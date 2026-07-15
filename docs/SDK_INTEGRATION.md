# lw.OpenCVDNN.PPOCR SDK 接入说明

## 文件说明

发布包的 `sdk` 目录包含：

- `native/lw.OpenCVDNN.PPOCR.dll`：x64、CPU-only 原生 OCR 动态库，静态集成 OpenCV 5 与 C++ 运行时。
- `native/lw.OpenCVDNN.PPOCR.lib`：C/C++ 隐式链接导入库。
- `native/ppocr_api.h`：稳定的 C ABI 头文件，可供 C、C++、C#、Python、Delphi、Java/JNA 等调用。
- `csharp/NativeOcr.cs`：兼容 .NET Framework 3.5 的 P/Invoke 参考封装。

DLL 版本：`1.1.0.0`。作者：天天代码码天天，QQ：819069052。

## 部署要求

1. 仅支持 x64 进程，调用程序必须编译为 `x64`，不能使用 `Any CPU` 或 `x86`。
2. 将 DLL 放在调用程序 EXE 同目录，或放入系统 DLL 搜索路径。
3. 模型与字典可放在任意目录，调用 `ppocr_create_ex_w` 时传入 UTF-16 绝对路径即可，中文路径受支持。
4. 本版本只使用 OpenCV DNN CPU 后端，不需要 CUDA、cuDNN、OpenVINO 或独立 OpenCV DLL。

## 生命周期与内存

1. 先调用 `ppocr_config_init`，再填写路径和需要覆盖的参数。
2. `ppocr_create_ex_w` 成功后返回 `ppocr_handle`，使用完必须调用 `ppocr_destroy`。
3. OCR 返回的 `utf8_json` 属于 DLL，读取后必须调用 `ppocr_free`，不能使用语言自身的 `free/delete`。
4. 同一个句柄的 OCR 调用由 DLL 串行保护；多个独立句柄可由上层自行并发调度。
5. 不要在 OCR 调用尚未结束时销毁句柄。发布包中的 C# 封装已经处理该约束。

`cpu_threads=0` 表示保留 OpenCV 默认线程策略；大于 0 时调用 `cv::setNumThreads` 设置进程级 OpenCV 线程数。多实例或 HTTP 高并发场景建议从 `1`、`2`、`4` 分别实测，不宜简单设置为逻辑核心总数。

## C/C++ 最小调用流程

```cpp
#include "ppocr_api.h"

ppocr_config_w config{};
ppocr_config_init(&config);
config.det_model_path = L"inference/PP-OCRv6_tiny_det.onnx";
config.rec_model_path = L"inference/PP-OCRv6_tiny_rec.onnx";
config.rec_dict_path = L"inference/PP-OCRv6_tiny_rec_dict.txt";
config.rec_predictor_num = 1;
config.cpu_threads = 0;

wchar_t error[512]{};
ppocr_handle engine = nullptr;
if (ppocr_create_ex_w(&config, &engine, error, 512) != 0) {
    // 记录 error 后退出
}

// encoded_data/encoded_size 是 JPG、PNG、BMP 等完整编码文件的字节数据。
char* json = nullptr;
int32_t json_size = 0;
const int code = ppocr_ocr_encoded(engine, encoded_data, encoded_size,
    &json, &json_size, error, 512);
if (code == 0) {
    std::string result(json, json_size); // UTF-8 JSON
}
ppocr_free(json);
ppocr_destroy(engine);
```

返回 JSON：

```json
{
  "elapsed_ms": 123.456,
  "results": [
    {"text":"示例","score":0.998,"x1":10,"y1":20,"x2":80,"y2":20,"x3":80,"y3":45,"x4":10,"y4":45}
  ]
}
```

## 仅识别与 ROI 识别

如果输入已经是裁剪好的单行文字图片，初始化时可省略检测模型：

```cpp
ppocr_config_w config{};
ppocr_config_init(&config);
config.det_model_path = nullptr;
config.rec_model_path = L"inference/PP-OCRv6_tiny_rec.onnx";
config.rec_dict_path = L"inference/PP-OCRv6_tiny_rec_dict.txt";
```

使用 `ppocr_recognize_encoded` 识别单张裁剪图，或使用
`ppocr_recognize_encoded_batch` 批量识别。若调用方拥有原图中的固定区域，
可以传入左上、右上、右下、左下四点：

```cpp
ppocr_roi roi{};
ppocr_roi_init(&roi);
roi.id = 101;
roi.x1 = 120; roi.y1 = 80;
roi.x2 = 279; roi.y2 = 80;
roi.x3 = 279; roi.y3 = 124;
roi.x4 = 120; roi.y4 = 124;
roi.rotation = 0;

char* json = nullptr;
int32_t json_size = 0;
ppocr_recognize_rois_encoded(engine, encoded_data, encoded_size,
    &roi, 1, &json, &json_size, error, 512);
ppocr_free(json);
```

`rotation` 允许顺时针 `0/90/180/270`。坐标必须位于解码后的原图范围内。
多个 ROI 会批量推理并保持输入顺序；单个无效 ROI 在对应结果中返回局部错误，
不会影响其他有效区域。

## C# 使用

将 `sdk/csharp/NativeOcr.cs` 加入 x64 的 .NET Framework 项目，构造 `PPOCRConfig` 后使用 `using (NativeOcr engine = new NativeOcr(config))` 调用 `Recognize(File.ReadAllBytes(imagePath))`。仅识别和 ROI 场景分别调用 `RecognizeTextLine`、`RecognizeRegions`。该封装已处理数组固定、UTF-8 JSON、原生返回内存释放和句柄释放。

## 错误码

- `0`：成功。
- `-1`：参数、结构体尺寸、图片尺寸或步长无效。
- `-2`：OpenCV 模型加载、图片解码、推理或结果内存分配失败。
- `-3/-4`：其他 C++ 异常或未知异常。

发生错误时优先读取 UTF-16 `error_message`。DLL 初始化会固定输出产品版本和作者信息；控制台使用 Unicode/UTF-8 输出，重定向到文件时为 UTF-8。
