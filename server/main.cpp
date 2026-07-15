#define _SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING

#include <windows.h>
#include <objbase.h>

#ifndef PPOCR_HTTP_THREAD_POOL_COUNT
#define PPOCR_HTTP_THREAD_POOL_COUNT 8
#endif
#define CPPHTTPLIB_THREAD_POOL_COUNT PPOCR_HTTP_THREAD_POOL_COUNT

#include <algorithm>
#include <chrono>
#include <cctype>
#include <cstdio>
#include <functional>
#include <fstream>
#include <iostream>
#include <memory>
#include <mutex>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>

#include "base64.h"
#include "httplib.h"
#include "ppocr_api.h"
#include "spdlog/sinks/daily_file_sink.h"
#include "spdlog/sinks/rotating_file_sink.h"
#include "spdlog/sinks/stdout_color_sinks.h"
#include "spdlog/spdlog.h"

#include "src/json.cpp"

using json = nlohmann::json;

namespace {

constexpr const char* kServiceName = "lw.OpenCVDNN.PPOCR.HttpServer";
constexpr const char* kDisplayName = "lw OpenCVDNN PPOCR HTTP Server";
constexpr const char* kAuthor = u8"天天代码码天天，QQ：819069052";

struct AppConfig {
    std::string host = "0.0.0.0";
    int port = 8080;
    std::string mode = "both";
    std::string det_model;
    std::string rec_model;
    std::string rec_dict;
    std::string cls_model;
    int limit_side_len = 960;
    double det_db_thresh = 0.3;
    double det_db_box_thresh = 0.6;
    double det_db_unclip_ratio = 1.5;
    bool use_dilation = false;
    bool use_angle_cls = false;
    bool cls = false;
    double cls_thresh = 0.9;
    int cls_batch_num = 1;
    int rec_batch_num =
#ifdef PPOCR_X86_LOW_MEMORY
        4;
#else
        8;
#endif
    int rec_img_h = 48;
    int rec_img_w = 320;
    int rec_predictor_num =
#ifdef PPOCR_X86_LOW_MEMORY
        1;
#else
        4;
#endif
    int cpu_threads =
#ifdef PPOCR_X86_LOW_MEMORY
        2;
#else
        0;
#endif
    std::wstring exe_dir;
};

struct OcrEngine {
    ppocr_handle handle = nullptr;

    ~OcrEngine() {
        if (handle) {
            ppocr_destroy(handle);
            spdlog::info("OCR engine destroyed");
        }
    }
};

AppConfig g_config;
OcrEngine g_engine;
std::unique_ptr<httplib::Server> g_server;
SERVICE_STATUS_HANDLE g_service_status_handle = nullptr;
SERVICE_STATUS g_service_status = {};

std::wstring GetExeDir() {
    wchar_t path[MAX_PATH] = {};
    const DWORD len = GetModuleFileNameW(nullptr, path, MAX_PATH);
    if (len == 0 || len == MAX_PATH) {
        wchar_t current[MAX_PATH] = {};
        GetCurrentDirectoryW(MAX_PATH, current);
        return current;
    }
    std::wstring result(path);
    const auto slash = result.find_last_of(L"\\/");
    return slash == std::wstring::npos ? L"." : result.substr(0, slash);
}

std::wstring Utf8ToWide(const std::string& value) {
    if (value.empty()) {
        return {};
    }
    const int size = MultiByteToWideChar(CP_UTF8, 0, value.c_str(), -1, nullptr, 0);
    if (size <= 0) {
        const int acp_size = MultiByteToWideChar(CP_ACP, 0, value.c_str(), -1, nullptr, 0);
        if (acp_size <= 0) {
            return {};
        }
        std::wstring result(static_cast<size_t>(acp_size), L'\0');
        MultiByteToWideChar(CP_ACP, 0, value.c_str(), -1, result.data(), acp_size);
        result.resize(static_cast<size_t>(acp_size - 1));
        return result;
    }
    std::wstring result(static_cast<size_t>(size), L'\0');
    MultiByteToWideChar(CP_UTF8, 0, value.c_str(), -1, result.data(), size);
    result.resize(static_cast<size_t>(size - 1));
    return result;
}

std::string WideToUtf8(const std::wstring& value) {
    if (value.empty()) {
        return {};
    }
    const int size = WideCharToMultiByte(CP_UTF8, 0, value.c_str(), -1, nullptr, 0, nullptr, nullptr);
    if (size <= 0) {
        return {};
    }
    std::string result(static_cast<size_t>(size), '\0');
    WideCharToMultiByte(CP_UTF8, 0, value.c_str(), -1, result.data(), size, nullptr, nullptr);
    result.resize(static_cast<size_t>(size - 1));
    return result;
}

bool IsAscii(const std::string& value) {
    return std::all_of(value.begin(), value.end(), [](unsigned char c) {
        return c < 0x80;
    });
}

std::wstring GetFullPath(const std::wstring& path) {
    DWORD needed = GetFullPathNameW(path.c_str(), 0, nullptr, nullptr);
    if (needed == 0) {
        return path;
    }
    std::wstring result(needed, L'\0');
    DWORD written = GetFullPathNameW(path.c_str(), needed, result.data(), nullptr);
    if (written == 0) {
        return path;
    }
    result.resize(written);
    return result;
}

bool IsAbsolutePath(const std::wstring& path) {
    return (path.size() > 2 && path[1] == L':' && (path[2] == L'\\' || path[2] == L'/')) ||
        (path.size() > 1 && path[0] == L'\\' && path[1] == L'\\');
}

std::wstring CombinePath(const std::wstring& base, const std::wstring& child) {
    if (child.empty()) {
        return base;
    }
    if (IsAbsolutePath(child)) {
        return child;
    }
    if (base.empty() || base.back() == L'\\' || base.back() == L'/') {
        return base + child;
    }
    return base + L"\\" + child;
}

bool FileExists(const std::wstring& path) {
    DWORD attrs = GetFileAttributesW(path.c_str());
    return attrs != INVALID_FILE_ATTRIBUTES && (attrs & FILE_ATTRIBUTE_DIRECTORY) == 0;
}

bool DirectoryExists(const std::wstring& path) {
    DWORD attrs = GetFileAttributesW(path.c_str());
    return attrs != INVALID_FILE_ATTRIBUTES && (attrs & FILE_ATTRIBUTE_DIRECTORY) != 0;
}

void EnsureDirectory(const std::wstring& path) {
    if (path.empty() || DirectoryExists(path)) {
        return;
    }
    const auto parent_pos = path.find_last_of(L"\\/");
    if (parent_pos != std::wstring::npos) {
        const auto parent = path.substr(0, parent_pos);
        if (!parent.empty() && parent.back() != L':') {
            EnsureDirectory(parent);
        }
    }
    if (!CreateDirectoryW(path.c_str(), nullptr) && GetLastError() != ERROR_ALREADY_EXISTS) {
        throw std::runtime_error("create directory failed");
    }
}

std::wstring GetFileName(const std::wstring& path) {
    const auto slash = path.find_last_of(L"\\/");
    return slash == std::wstring::npos ? path : path.substr(slash + 1);
}

std::wstring GetParentPath(const std::wstring& path) {
    const auto slash = path.find_last_of(L"\\/");
    return slash == std::wstring::npos ? L"." : path.substr(0, slash);
}

unsigned long long GetFileSizeValue(const std::wstring& path) {
    WIN32_FILE_ATTRIBUTE_DATA data = {};
    if (!GetFileAttributesExW(path.c_str(), GetFileExInfoStandard, &data)) {
        return 0;
    }
    ULARGE_INTEGER size = {};
    size.HighPart = data.nFileSizeHigh;
    size.LowPart = data.nFileSizeLow;
    return size.QuadPart;
}

unsigned long long GetFileWriteTimeValue(const std::wstring& path) {
    WIN32_FILE_ATTRIBUTE_DATA data = {};
    if (!GetFileAttributesExW(path.c_str(), GetFileExInfoStandard, &data)) {
        return 0;
    }
    ULARGE_INTEGER time = {};
    time.HighPart = data.ftLastWriteTime.dwHighDateTime;
    time.LowPart = data.ftLastWriteTime.dwLowDateTime;
    return time.QuadPart;
}

std::string TryGetShortAsciiPath(const std::wstring& path) {
    DWORD needed = GetShortPathNameW(path.c_str(), nullptr, 0);
    if (needed == 0) {
        return {};
    }
    std::wstring short_path(needed, L'\0');
    DWORD written = GetShortPathNameW(path.c_str(), short_path.data(), needed);
    if (written == 0) {
        return {};
    }
    short_path.resize(written);
    auto utf8 = WideToUtf8(short_path);
    return IsAscii(utf8) ? utf8 : std::string();
}

std::wstring GetProgramDataDir() {
    wchar_t buffer[MAX_PATH] = {};
    DWORD len = GetEnvironmentVariableW(L"ProgramData", buffer, MAX_PATH);
    if (len > 0 && len < MAX_PATH) {
        return buffer;
    }
    return L"C:\\ProgramData";
}

std::wstring GetAsciiAssetCacheDir() {
    auto dir = CombinePath(CombinePath(GetProgramDataDir(), L"lw.OpenCVDNN.PPOCR.HttpServer"), L"assets");
    EnsureDirectory(dir);
    return dir;
}

std::string MakeNativeReadableFilePath(const std::string& utf8_path) {
    std::wstring source = GetFullPath(Utf8ToWide(utf8_path));
    const auto normalized_utf8 = WideToUtf8(source);
    if (IsAscii(normalized_utf8)) {
        return normalized_utf8;
    }

    if (!FileExists(source)) {
        return normalized_utf8;
    }

    if (auto short_path = TryGetShortAsciiPath(source); !short_path.empty()) {
        return short_path;
    }

    const auto hash = std::to_wstring(static_cast<unsigned long long>(std::hash<std::wstring>{}(GetParentPath(source))));
    auto cache_dir = CombinePath(GetAsciiAssetCacheDir(), hash);
    EnsureDirectory(cache_dir);
    auto target = CombinePath(cache_dir, GetFileName(source));
    const bool should_copy =
        !FileExists(target) ||
        GetFileSizeValue(target) != GetFileSizeValue(source) ||
        GetFileWriteTimeValue(target) < GetFileWriteTimeValue(source);
    if (should_copy) {
        if (!CopyFileW(source.c_str(), target.c_str(), FALSE)) {
            throw std::runtime_error("copy model asset to ascii cache failed");
        }
    }

    if (auto short_path = TryGetShortAsciiPath(target); !short_path.empty()) {
        return short_path;
    }
    const auto target_utf8 = WideToUtf8(target);
    return IsAscii(target_utf8) ? target_utf8 : normalized_utf8;
}

std::string NormalizePath(std::string value, const std::wstring& base) {
    if (value.empty()) {
        return value;
    }
    std::wstring path = Utf8ToWide(value);
    if (!IsAbsolutePath(path)) {
        path = CombinePath(base, path);
    }
    return WideToUtf8(GetFullPath(path));
}

std::string GetArgValue(int argc, char* argv[], const std::string& name, const std::string& fallback) {
    const std::string prefix = name + "=";
    for (int i = 1; i < argc; ++i) {
        const std::string arg = argv[i];
        if (arg == name && i + 1 < argc) {
            return argv[i + 1];
        }
        if (arg.rfind(prefix, 0) == 0) {
            return arg.substr(prefix.size());
        }
    }
    return fallback;
}

bool HasArg(int argc, char* argv[], const std::string& name) {
    for (int i = 1; i < argc; ++i) {
        if (argv[i] == name) {
            return true;
        }
    }
    return false;
}

int ToInt(const std::string& value, int fallback) {
    try {
        return std::stoi(value);
    } catch (...) {
        return fallback;
    }
}

double ToDouble(const std::string& value, double fallback) {
    try {
        return std::stod(value);
    } catch (...) {
        return fallback;
    }
}

bool ToBool(const std::string& value, bool fallback) {
    std::string s = value;
    std::transform(s.begin(), s.end(), s.begin(), [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
    if (s == "1" || s == "true" || s == "yes" || s == "on") {
        return true;
    }
    if (s == "0" || s == "false" || s == "no" || s == "off") {
        return false;
    }
    return fallback;
}

AppConfig ParseConfig(int argc, char* argv[]) {
    AppConfig cfg;
    cfg.exe_dir = GetExeDir();
    cfg.host = GetArgValue(argc, argv, "--host", cfg.host);
    cfg.port = ToInt(GetArgValue(argc, argv, "--port", std::to_string(cfg.port)), cfg.port);
    cfg.mode = GetArgValue(argc, argv, "--mode", cfg.mode);
    std::transform(cfg.mode.begin(), cfg.mode.end(), cfg.mode.begin(),
        [](unsigned char c) { return static_cast<char>(std::tolower(c)); });

    cfg.det_model = GetArgValue(argc, argv, "--det_model", "inference/PP-OCRv6_tiny_det.onnx");
    cfg.rec_model = GetArgValue(argc, argv, "--rec_model", "inference/PP-OCRv6_tiny_rec.onnx");
    cfg.rec_dict = GetArgValue(argc, argv, "--rec_dict", "inference/PP-OCRv6_tiny_rec_dict.txt");
    cfg.cls_model = GetArgValue(argc, argv, "--cls_model", "inference/PP-OCRv5_mobile_cls_onnx.onnx");

    cfg.limit_side_len = ToInt(GetArgValue(argc, argv, "--limit_side_len", std::to_string(cfg.limit_side_len)), cfg.limit_side_len);
    cfg.det_db_thresh = ToDouble(GetArgValue(argc, argv, "--det_db_thresh", std::to_string(cfg.det_db_thresh)), cfg.det_db_thresh);
    cfg.det_db_box_thresh = ToDouble(GetArgValue(argc, argv, "--det_db_box_thresh", std::to_string(cfg.det_db_box_thresh)), cfg.det_db_box_thresh);
    cfg.det_db_unclip_ratio = ToDouble(GetArgValue(argc, argv, "--det_db_unclip_ratio", std::to_string(cfg.det_db_unclip_ratio)), cfg.det_db_unclip_ratio);
    cfg.use_dilation = ToBool(GetArgValue(argc, argv, "--use_dilation", cfg.use_dilation ? "true" : "false"), cfg.use_dilation);
    cfg.cls = ToBool(GetArgValue(argc, argv, "--cls", cfg.cls ? "true" : "false"), cfg.cls);
    cfg.use_angle_cls = ToBool(GetArgValue(argc, argv, "--use_angle_cls", cfg.use_angle_cls ? "true" : "false"), cfg.use_angle_cls);
    cfg.cls_thresh = ToDouble(GetArgValue(argc, argv, "--cls_thresh", std::to_string(cfg.cls_thresh)), cfg.cls_thresh);
    cfg.cls_batch_num = ToInt(GetArgValue(argc, argv, "--cls_batch_num", std::to_string(cfg.cls_batch_num)), cfg.cls_batch_num);
    cfg.rec_batch_num = ToInt(GetArgValue(argc, argv, "--rec_batch_num", std::to_string(cfg.rec_batch_num)), cfg.rec_batch_num);
    cfg.rec_img_h = ToInt(GetArgValue(argc, argv, "--rec_img_h", std::to_string(cfg.rec_img_h)), cfg.rec_img_h);
    cfg.rec_img_w = ToInt(GetArgValue(argc, argv, "--rec_img_w", std::to_string(cfg.rec_img_w)), cfg.rec_img_w);
    cfg.rec_predictor_num = ToInt(GetArgValue(argc, argv, "--rec_predictor_num", std::to_string(cfg.rec_predictor_num)), cfg.rec_predictor_num);
    cfg.cpu_threads = ToInt(GetArgValue(argc, argv, "--cpu_threads", std::to_string(cfg.cpu_threads)), cfg.cpu_threads);

    cfg.det_model = NormalizePath(cfg.det_model, cfg.exe_dir);
    cfg.rec_model = NormalizePath(cfg.rec_model, cfg.exe_dir);
    cfg.rec_dict = NormalizePath(cfg.rec_dict, cfg.exe_dir);
    cfg.cls_model = NormalizePath(cfg.cls_model, cfg.exe_dir);
    return cfg;
}

void SetupLogger(const std::wstring& exe_dir) {
    const auto log_dir = CombinePath(exe_dir, L"logs");
    EnsureDirectory(log_dir);
    std::wstring native_log_dir = log_dir;
    auto short_log_dir = TryGetShortAsciiPath(log_dir);
    if (short_log_dir.empty() && !IsAscii(WideToUtf8(log_dir))) {
        native_log_dir = CombinePath(CombinePath(GetProgramDataDir(), L"lw.OpenCVDNN.PPOCR.HttpServer"), L"logs");
        EnsureDirectory(native_log_dir);
    }
    const auto log_dir_text = short_log_dir.empty() ? WideToUtf8(native_log_dir) : short_log_dir;

    std::vector<spdlog::sink_ptr> sinks;
    sinks.push_back(std::make_shared<spdlog::sinks::stdout_color_sink_mt>());
    sinks.push_back(std::make_shared<spdlog::sinks::daily_file_sink_mt>(log_dir_text + "\\ocr_daily.log", 0, 0, false, 7));
    sinks.push_back(std::make_shared<spdlog::sinks::rotating_file_sink_mt>(log_dir_text + "\\ocr_server.log", 1024 * 1024, 7));

    auto logger = std::make_shared<spdlog::logger>("ocr_http", sinks.begin(), sinks.end());
    logger->set_level(spdlog::level::info);
    logger->flush_on(spdlog::level::info);
    spdlog::set_default_logger(logger);
    spdlog::set_pattern("[%Y-%m-%d %H:%M:%S.%e] [%l] %v");
}

void PrintStartupInfo(const AppConfig& cfg) {
    spdlog::info("=======================================");
    spdlog::info("lw.OpenCVDNN.PPOCR.HttpServer 1.1.0.0");
    spdlog::info("Author: {}", kAuthor);
    spdlog::info("HTTP: http://{}:{}/", cfg.host, cfg.port);
    spdlog::info("mode: {}", cfg.mode);
    spdlog::info("det_model: {}", cfg.det_model);
    spdlog::info("rec_model: {}", cfg.rec_model);
    spdlog::info("rec_dict: {}", cfg.rec_dict);
    spdlog::info("limit_side_len: {}", cfg.limit_side_len);
    spdlog::info("det_db_thresh: {}, det_db_box_thresh: {}, det_db_unclip_ratio: {}", cfg.det_db_thresh, cfg.det_db_box_thresh, cfg.det_db_unclip_ratio);
    spdlog::info("rec_batch_num: {}, rec_img_h: {}, rec_img_w: {}, rec_predictor_num: {}", cfg.rec_batch_num, cfg.rec_img_h, cfg.rec_img_w, cfg.rec_predictor_num);
    spdlog::info("cpu_threads: {}, process_arch: {}-bit, HTTP workers: {}",
        cfg.cpu_threads, sizeof(void*) * 8, CPPHTTPLIB_THREAD_POOL_COUNT);
#ifdef PPOCR_X86_LOW_MEMORY
    spdlog::info("mode: x86 low-memory / low-concurrency");
#endif
    spdlog::info("backend: OpenCV DNN CPU");
    spdlog::info("=======================================");
}

void InitOcr(const AppConfig& cfg) {
    if (cfg.mode != "ocr" && cfg.mode != "rec" && cfg.mode != "both") {
        throw std::runtime_error("--mode must be ocr, rec or both");
    }
    const std::wstring det = Utf8ToWide(cfg.det_model);
    const std::wstring rec = Utf8ToWide(cfg.rec_model);
    const std::wstring dict = Utf8ToWide(cfg.rec_dict);
    const std::wstring cls_model = Utf8ToWide(cfg.cls_model);
    wchar_t error[512] = {};
    ppocr_config_w native = {};
    ppocr_config_init(&native);
    native.det_model_path = cfg.mode == "rec" ? nullptr : det.c_str();
    native.rec_model_path = rec.c_str();
    native.rec_dict_path = dict.c_str();
    native.cls_model_path = cfg.mode == "rec" ? nullptr : cls_model.c_str();
    native.limit_side_len = cfg.limit_side_len;
    native.det_db_thresh = cfg.det_db_thresh;
    native.det_db_box_thresh = cfg.det_db_box_thresh;
    native.det_db_unclip_ratio = cfg.det_db_unclip_ratio;
    native.use_dilation = cfg.use_dilation ? 1 : 0;
    native.use_angle_cls = cfg.mode != "rec" && cfg.use_angle_cls ? 1 : 0;
    native.cls_thresh = cfg.cls_thresh;
    native.cls_batch_num = cfg.cls_batch_num;
    native.rec_batch_num = cfg.rec_batch_num;
    native.rec_img_h = cfg.rec_img_h;
    native.rec_img_w = cfg.rec_img_w;
    native.rec_predictor_num = cfg.rec_predictor_num;
    native.cpu_threads = cfg.cpu_threads;

    const int code = ppocr_create_ex_w(&native, &g_engine.handle, error, 512);
    if (code != 0) {
        throw std::runtime_error("init OCR failed: " + WideToUtf8(error));
    }
    spdlog::info("OCR init success, native version: {}", ppocr_get_version());
}

std::string ReadTextFile(const std::wstring& path) {
    FILE* file = nullptr;
    if (_wfopen_s(&file, path.c_str(), L"rb") != 0 || file == nullptr) {
        throw std::runtime_error("open file failed");
    }
    std::unique_ptr<FILE, decltype(&fclose)> holder(file, fclose);
    std::fseek(file, 0, SEEK_END);
    const long size = std::ftell(file);
    std::fseek(file, 0, SEEK_SET);
    std::string content(static_cast<size_t>(std::max(0L, size)), '\0');
    if (!content.empty()) {
        std::fread(content.data(), 1, content.size(), file);
    }
    return content;
}

std::string ExtractImageBase64(const httplib::Request& req) {
    if (req.has_param("imageBase64")) {
        return req.get_param_value("imageBase64");
    }
    if (req.has_param("image")) {
        return req.get_param_value("image");
    }
    if (req.get_header_value("Content-Type").find("application/json") != std::string::npos) {
        auto body = json::parse(req.body);
        return body["imageBase64"].get<std::string>();
    }
    return req.body;
}

void StripBase64PrefixAndWhitespace(std::string& value) {
    const auto comma = value.find(',');
    if (value.rfind("data:", 0) == 0 && comma != std::string::npos) {
        value = value.substr(comma + 1);
    }
    value.erase(std::remove_if(value.begin(), value.end(), [](unsigned char c) {
        return c == '\r' || c == '\n' || c == '\t' || c == ' ';
    }), value.end());
}

std::string OcrJsonResponse(const httplib::Request& req) {
    auto image_base64 = ExtractImageBase64(req);
    StripBase64PrefixAndWhitespace(image_base64);
    if (image_base64.empty()) {
        throw std::runtime_error("imageBase64 is empty");
    }

    const std::string bytes = base64_decode(image_base64, true);
    if (bytes.empty()) throw std::runtime_error("decode base64 failed");

    wchar_t error[512] = {};
    char* result = nullptr;
    int result_len = 0;
    const int code = ppocr_ocr_encoded(
        g_engine.handle,
        reinterpret_cast<const uint8_t*>(bytes.data()),
        static_cast<int>(bytes.size()),
        &result,
        &result_len,
        error,
        512);

    std::string raw_result;
    if (result != nullptr && result_len > 0) raw_result.assign(result, result + result_len);
    if (result != nullptr) ppocr_free(result);

    if (code != 0) throw std::runtime_error(WideToUtf8(error));
    json response = json::parse(raw_result);
    response["code"] = 0;
    response["msg"] = "ok";
    spdlog::info("OCR request finished, elapsed_ms={}, boxes={}",
        response["elapsed_ms"].get<double>(), response["results"].size());
    return response.dump();
}

const json& RequireField(const json& value, const char* name) {
    const auto it = value.find(name);
    if (it == value.end()) throw std::runtime_error(std::string("missing field: ") + name);
    return *it;
}

int OptionalInt(const json& value, const char* name, int fallback) {
    const auto it = value.find(name);
    return it == value.end() ? fallback : it->get<int>();
}

std::string DecodeBase64Image(std::string image_base64) {
    StripBase64PrefixAndWhitespace(image_base64);
    if (image_base64.empty()) throw std::runtime_error("imageBase64 is empty");
    std::string bytes = base64_decode(image_base64, true);
    if (bytes.empty()) throw std::runtime_error("decode base64 failed");
    return bytes;
}

std::string FinishNativeResponse(int code, char* result, int result_len,
    const wchar_t* error, const char* operation) {
    std::string raw_result;
    if (result != nullptr && result_len > 0) raw_result.assign(result, result + result_len);
    if (result != nullptr) ppocr_free(result);
    if (code != 0) throw std::runtime_error(WideToUtf8(error));
    if (raw_result.empty()) throw std::runtime_error(std::string(operation) + " returned an empty result");
    json response = json::parse(raw_result);
    response["code"] = 0;
    response["msg"] = "ok";
    return response.dump();
}

std::string RecognizeJsonResponse(const httplib::Request& req) {
    const std::string bytes = DecodeBase64Image(ExtractImageBase64(req));
    wchar_t error[512] = {};
    char* result = nullptr;
    int result_len = 0;
    const int code = ppocr_recognize_encoded(
        g_engine.handle,
        reinterpret_cast<const uint8_t*>(bytes.data()),
        static_cast<int>(bytes.size()),
        &result, &result_len, error, 512);
    return FinishNativeResponse(code, result, result_len, error, "recognition");
}

std::vector<ppocr_roi> ParseRois(const json& body) {
    const json& items = RequireField(body, "rois");
    if (!items.is_array() || items.empty()) throw std::runtime_error("rois must be a non-empty array");
#ifdef PPOCR_X86_LOW_MEMORY
    const size_t max_rois = 64;
#else
    const size_t max_rois = 256;
#endif
    if (items.size() > max_rois) throw std::runtime_error("too many ROIs");

    std::vector<ppocr_roi> rois;
    rois.reserve(items.size());
    for (size_t i = 0; i < items.size(); ++i) {
        const json& item = items[i];
        if (!item.is_object()) throw std::runtime_error("each ROI must be an object");
        ppocr_roi roi;
        ppocr_roi_init(&roi);
        roi.id = OptionalInt(item, "id", static_cast<int>(i));
        roi.rotation = OptionalInt(item, "rotation", 0);

        const auto points_it = item.find("points");
        if (points_it != item.end()) {
            const json& points = *points_it;
            if (!points.is_array() || points.size() != 4) {
                throw std::runtime_error("ROI points must contain exactly four points");
            }
            int* coordinates[8] = {
                &roi.x1, &roi.y1, &roi.x2, &roi.y2,
                &roi.x3, &roi.y3, &roi.x4, &roi.y4
            };
            for (size_t p = 0; p < 4; ++p) {
                *coordinates[p * 2] = RequireField(points[p], "x").get<int>();
                *coordinates[p * 2 + 1] = RequireField(points[p], "y").get<int>();
            }
        }
        else {
            const json* rectangle = &item;
            const auto rect_it = item.find("rect");
            if (rect_it != item.end()) rectangle = &(*rect_it);
            const int64_t x = RequireField(*rectangle, "x").get<int64_t>();
            const int64_t y = RequireField(*rectangle, "y").get<int64_t>();
            const int64_t width = RequireField(*rectangle, "width").get<int64_t>();
            const int64_t height = RequireField(*rectangle, "height").get<int64_t>();
            if (width <= 0 || height <= 0 || x < INT32_MIN || y < INT32_MIN ||
                x + width - 1 > INT32_MAX || y + height - 1 > INT32_MAX) {
                throw std::runtime_error("invalid ROI rectangle");
            }
            roi.x1 = roi.x4 = static_cast<int32_t>(x);
            roi.x2 = roi.x3 = static_cast<int32_t>(x + width - 1);
            roi.y1 = roi.y2 = static_cast<int32_t>(y);
            roi.y3 = roi.y4 = static_cast<int32_t>(y + height - 1);
        }
        rois.push_back(roi);
    }
    return rois;
}

std::string RecognizeRoisJsonResponse(const httplib::Request& req) {
    if (req.get_header_value("Content-Type").find("application/json") == std::string::npos) {
        throw std::runtime_error("Content-Type must be application/json");
    }
    const json body = json::parse(req.body);
    const std::string image_base64 = RequireField(body, "imageBase64").get<std::string>();
    const std::string bytes = DecodeBase64Image(image_base64);
    const std::vector<ppocr_roi> rois = ParseRois(body);

    wchar_t error[512] = {};
    char* result = nullptr;
    int result_len = 0;
    const int code = ppocr_recognize_rois_encoded(
        g_engine.handle,
        reinterpret_cast<const uint8_t*>(bytes.data()),
        static_cast<int>(bytes.size()),
        rois.data(), static_cast<int>(rois.size()),
        &result, &result_len, error, 512);
    return FinishNativeResponse(code, result, result_len, error, "ROI recognition");
}

void SetApiError(httplib::Response& res, const std::exception& error) {
    json response;
    response["code"] = -1;
    response["msg"] = error.what();
    response["elapsed_ms"] = 0;
    response["results"] = json::array();
    res.status = 400;
    res.set_content(response.dump(), "application/json; charset=utf-8");
}

void BuildRoutes(httplib::Server& server, const AppConfig& cfg) {
    server.Get("/", [&cfg](const httplib::Request&, httplib::Response& res) {
        const auto html = ReadTextFile(CombinePath(CombinePath(cfg.exe_dir, L"www"), L"index.html"));
        res.set_content(html, "text/html; charset=utf-8");
    });

    server.Get("/health", [](const httplib::Request&, httplib::Response& res) {
        res.set_content("{\"code\":0,\"msg\":\"ok\"}", "application/json; charset=utf-8");
    });

    if (cfg.mode != "rec") {
        server.Post("/api/ocr", [](const httplib::Request& req, httplib::Response& res) {
            try {
                res.set_content(OcrJsonResponse(req), "application/json; charset=utf-8");
            } catch (const std::exception& e) {
                spdlog::error("OCR request failed: {}", e.what());
                SetApiError(res, e);
            }
        });
    }

    if (cfg.mode != "ocr") {
        server.Post("/api/recognize", [](const httplib::Request& req, httplib::Response& res) {
            try {
                res.set_content(RecognizeJsonResponse(req), "application/json; charset=utf-8");
            } catch (const std::exception& e) {
                spdlog::error("Recognition request failed: {}", e.what());
                SetApiError(res, e);
            }
        });

        server.Post("/api/recognize-rois", [](const httplib::Request& req, httplib::Response& res) {
            try {
                res.set_content(RecognizeRoisJsonResponse(req), "application/json; charset=utf-8");
            } catch (const std::exception& e) {
                spdlog::error("ROI recognition request failed: {}", e.what());
                SetApiError(res, e);
            }
        });
    }

    server.set_exception_handler([](const httplib::Request&, httplib::Response& res, std::exception_ptr ep) {
        try {
            if (ep) {
                std::rethrow_exception(ep);
            }
        } catch (const std::exception& e) {
            spdlog::error("HTTP handler failed: {}", e.what());
        }
        res.status = 500;
        res.set_content("{\"code\":-1,\"msg\":\"server error\"}", "application/json; charset=utf-8");
    });
}

int RunServer(const AppConfig& cfg) {
    InitOcr(cfg);

    g_server = std::make_unique<httplib::Server>();
    g_server->set_read_timeout(60, 0);
    g_server->set_write_timeout(60, 0);
#ifdef PPOCR_X86_LOW_MEMORY
    g_server->set_payload_max_length(10 * 1024 * 1024);
#else
    g_server->set_payload_max_length(50 * 1024 * 1024);
#endif
    BuildRoutes(*g_server, cfg);

    spdlog::info("Server listening on {}:{}", cfg.host, cfg.port);
    const bool ok = g_server->listen(cfg.host.c_str(), cfg.port);
    if (!ok) {
        spdlog::error("Server listen failed on {}:{}", cfg.host, cfg.port);
        return 2;
    }

    spdlog::info("Server stopped");
    return 0;
}

void SetServiceStatus(DWORD state, DWORD exit_code = NO_ERROR, DWORD wait_hint = 0) {
    if (!g_service_status_handle) {
        return;
    }
    g_service_status.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
    g_service_status.dwCurrentState = state;
    g_service_status.dwWin32ExitCode = exit_code;
    g_service_status.dwWaitHint = wait_hint;
    g_service_status.dwControlsAccepted = state == SERVICE_RUNNING ? SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN : 0;
    ::SetServiceStatus(g_service_status_handle, &g_service_status);
}

DWORD WINAPI ServiceControlHandler(DWORD control, DWORD, LPVOID, LPVOID) {
    if (control == SERVICE_CONTROL_STOP || control == SERVICE_CONTROL_SHUTDOWN) {
        SetServiceStatus(SERVICE_STOP_PENDING, NO_ERROR, 3000);
        if (g_server) {
            g_server->stop();
        }
        SetServiceStatus(SERVICE_STOPPED);
        return NO_ERROR;
    }
    return NO_ERROR;
}

void WINAPI ServiceMain(DWORD, LPSTR*) {
    g_service_status_handle = RegisterServiceCtrlHandlerExA(kServiceName, ServiceControlHandler, nullptr);
    if (!g_service_status_handle) {
        return;
    }
    SetServiceStatus(SERVICE_START_PENDING, NO_ERROR, 3000);
    try {
        SetupLogger(g_config.exe_dir);
        PrintStartupInfo(g_config);
        SetServiceStatus(SERVICE_RUNNING);
        RunServer(g_config);
        SetServiceStatus(SERVICE_STOPPED);
    } catch (const std::exception& e) {
        spdlog::error("Service failed: {}", e.what());
        SetServiceStatus(SERVICE_STOPPED, ERROR_SERVICE_SPECIFIC_ERROR);
    }
}

int RunAsService() {
    SERVICE_TABLE_ENTRYA service_table[] = {
        {const_cast<char*>(kServiceName), reinterpret_cast<LPSERVICE_MAIN_FUNCTIONA>(ServiceMain)},
        {nullptr, nullptr}
    };
    if (!StartServiceCtrlDispatcherA(service_table)) {
        const auto error = GetLastError();
        std::cerr << "StartServiceCtrlDispatcher failed: " << error << std::endl;
        if (error == ERROR_FAILED_SERVICE_CONTROLLER_CONNECT) {
            std::cerr << "--service can only be started by Windows Service Control Manager." << std::endl;
            std::cerr << "Run without --service for console mode, or install/start it with deploy\\install_service.bat." << std::endl;
        }
        return 1;
    }
    return 0;
}

void PrintUsage() {
    std::cout << "Usage: lw.OpenCVDNN.PPOCR.HttpServer.exe [options]\n"
              << "  --port 8080\n"
              << "  --host 0.0.0.0\n"
              << "  --mode both     ocr, rec or both\n"
              << "  --det_model inference/PP-OCRv6_tiny_det.onnx\n"
              << "  --rec_model inference/PP-OCRv6_tiny_rec.onnx\n"
              << "  --rec_dict inference/PP-OCRv6_tiny_rec_dict.txt\n"
              << "  --limit_side_len 960 --det_db_thresh 0.3 --det_db_box_thresh 0.6\n"
              << "  --rec_predictor_num 1 --cpu_threads 0\n"
              << "  --service    run under Windows Service Control Manager\n";
}

} // namespace

int main(int argc, char* argv[]) {
    // spdlog writes UTF-8 bytes; select the matching console code page first.
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);
    if (HasArg(argc, argv, "--help") || HasArg(argc, argv, "-h")) {
        PrintUsage();
        return 0;
    }

    g_config = ParseConfig(argc, argv);
    if (!HasArg(argc, argv, "--service")) {
        SetupLogger(g_config.exe_dir);
        PrintStartupInfo(g_config);
        try {
            return RunServer(g_config);
        } catch (const std::exception& e) {
            spdlog::error("Fatal error: {}", e.what());
            return 1;
        }
    }

    return RunAsService();
}
