#pragma once

#include <stdint.h>

#if defined(_WIN32)
#  if defined(PPOCR_NATIVE_EXPORTS)
#    define PPOCR_API __declspec(dllexport)
#  else
#    define PPOCR_API __declspec(dllimport)
#  endif
#  define PPOCR_CALL __cdecl
#else
#  define PPOCR_API
#  define PPOCR_CALL
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef void* ppocr_handle;

/* UTF-16 configuration for Windows callers. Initialize it with ppocr_config_init. */
typedef struct ppocr_config_w {
    int32_t struct_size;
    const wchar_t* det_model_path;
    const wchar_t* rec_model_path;
    const wchar_t* rec_dict_path;
    const wchar_t* cls_model_path;

    int32_t limit_side_len;
    double det_db_thresh;
    double det_db_box_thresh;
    double det_db_unclip_ratio;
    int32_t use_dilation;

    int32_t use_angle_cls;
    double cls_thresh;
    int32_t cls_batch_num;

    int32_t rec_batch_num;
    int32_t rec_img_h;
    int32_t rec_img_w;
    int32_t rec_predictor_num;
    int32_t cpu_threads;
} ppocr_config_w;

PPOCR_API void PPOCR_CALL ppocr_config_init(ppocr_config_w* config);

PPOCR_API int32_t PPOCR_CALL ppocr_create_w(
    const wchar_t* det_model_path,
    const wchar_t* rec_model_path,
    const wchar_t* rec_dict_path,
    int32_t cpu_threads,
    ppocr_handle* handle,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_create_ex_w(
    const ppocr_config_w* config,
    ppocr_handle* handle,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_ocr_encoded(
    ppocr_handle handle,
    const uint8_t* encoded_image,
    int32_t encoded_size,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_ocr_bgr(
    ppocr_handle handle,
    const uint8_t* pixels,
    int32_t width,
    int32_t height,
    int32_t channels,
    int32_t stride,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

/*
 * utf8_json is allocated by the DLL and must be released with ppocr_free.
 * A handle serializes its own OCR calls. Do not call ppocr_destroy while a call is active.
 */
PPOCR_API void PPOCR_CALL ppocr_free(void* memory);
PPOCR_API void PPOCR_CALL ppocr_destroy(ppocr_handle handle);
PPOCR_API const char* PPOCR_CALL ppocr_get_version(void);

#ifdef __cplusplus
}
#endif
