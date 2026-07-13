# lw.OpenCVDNN.PPOCR

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.0.0.1-green.svg)](https://github.com/lxw112190/lw.OpenCVDNN.PPOCR/releases)

**14MB 纯 CPU OCR 动态库，支持 .NET Framework 3.5，解压即用，兼容 Windows 7。**

- **作者：** 天天代码码天天 &nbsp;|&nbsp; **QQ：** 819069052
- **QQ 群：** C# 人工智能实践 &nbsp;|&nbsp; **群号：** 758616458
- **目标系统：** Windows 7 SP1 及以上（推荐 x64，同时提供 x86 版本）

> **发布模式：** 这是一个“源码 + 预编译二进制”的混合 SDK。HTTP 服务、
> 公共头文件和示例以 MIT 许可证提供源码；原生 OCR 引擎仅提供预编译二进制，
> 不包含其 C++ 源码。准确范围见 [DISTRIBUTION.md](DISTRIBUTION.md)。

---

## 为什么做这个项目

很多人以为 Windows 7 已经离开主流视野，就等于离开了生产现场。

但在工控机、产线终端、检测设备和老旧业务系统里，情况完全不同：设备运行稳定、改造成本高、上位机软件仍停留在 .NET Framework 3.5，既不能随意升级系统，也不适合安装一长串运行环境。

**这类设备真正需要的不是最复杂的技术栈，而是：**

- 保留现有 Windows 和上位机程序。
- 不安装 Python，不配置 CUDA，不折腾显卡驱动。
- 解压即可运行，离线也能部署。
- 面向 .NET Framework 3.5 的 C# 程序可以直接调用，其他支持 C FFI
  的语言也可以复用同一套 C ABI。
- 出错时能看到日志、模型路径和明确的初始化信息。

`lw.OpenCVDNN.PPOCR` 就是围绕这些约束做的一套工程化实现。

## 概览

| | |
|---|---|
| **推理引擎** | OpenCV 5 DNN（纯 CPU） |
| **OCR 模型** | PP-OCRv5 / PP-OCRv6 tiny / PP-OCRv6 small |
| **原生组件** | x64 C++ DLL 约 14 MB；x86 DLL 约 10 MB |
| **C# 目标** | .NET Framework 3.5 |
| **部署方式** | DLL 直调 / HTTP 服务 / Windows 服务 |
| **模型路径** | 支持 UTF-16 中文路径 |
| **额外运行时** | 零依赖——无需独立 OpenCV DLL、VC++ 运行库、Python、CUDA |

## 方案对比

不同方案各有价值。这里的选择标准不是"谁在所有环境里最强"，而是谁更适合老旧、离线、依赖受限的工控设备。

| 方案 | 典型部署依赖 | Win7 与老 .NET 接入 | GPU 要求 | 定位 |
|---|---|---|---|---|
| Python + PaddleOCR | Python、PaddlePaddle 及第三方包 | 通常需要进程或 HTTP 中转 | 可选 | 研发验证方便，现场部署较重 |
| OpenCvSharp + OpenCV | 托管组件与原生 OpenCV 库 | 需要匹配可用版本 | 不需要 | C# 开发方便，但组件数量更多 |
| ONNX Runtime | ONNX Runtime 动态库 | 取决于运行库版本和封装 | 可选 | 通用性好，需额外维护运行时 |
| TensorRT | CUDA、cuDNN、TensorRT 及驱动 | 不适合无 NVIDIA GPU 设备 | 必需 | NVIDIA 设备上追求极致速度 |
| **本项目** | **DLL + 模型 + 字典** | **.NET Framework 3.5 封装 + C ABI** | **不需要** | **纯 CPU、自包含部署、长周期工业软件** |

OpenCV DNN 不一定在所有硬件上最快。它的优势是依赖少、接口稳定、模型通用、部署路径清晰——恰恰是长维护周期工业软件最需要的。

## 调用链

```text
JPG / PNG / BMP 图片
      │
      ├── C#、C++ 等程序传入图片字节
      └── HTTP 客户端传入 Base64
                  │
                  ▼
       lw.OpenCVDNN.PPOCR.dll
                  │
         OpenCV 5 DNN CPU 推理
                  │
     文本检测 → 透视裁剪 → 方向分类（可选）→ 文字识别
                  │
                  ▼
 UTF-8 JSON：文字、置信度、四点坐标、OCR 耗时
```

## 发布包内容

```text
lw.OpenCVDNN.PPOCR.Win7-x64/
  LICENSE、DISTRIBUTION.md                项目许可与分发范围
  THIRD_PARTY_NOTICES.md、licenses/       第三方声明与许可证原文
  lw.OpenCVDNN.PPOCR.dll                 OCR 原生动态库（~14 MB，自包含）
  lw.OpenCVDNN.PPOCR.HttpServer.exe      HTTP 服务
  lw.OpenCVDNN.PPOCR.Win7.Test.exe       .NET Framework 3.5 WinForms 测试程序
  inference/                             PP-OCR ONNX 模型与字典
  www/                                   浏览器测试页面
  deploy/                                Windows 服务安装/卸载脚本
  sdk/
    native/                              DLL、导入库、ppocr_api.h
    csharp/                              NativeOcr.cs 参考封装
    SDK_INTEGRATION.md                   多语言接入说明
  test-images/                           测试图片
  start_console.bat                      一键启动 HTTP 服务
  start_winforms_test.bat                一键启动 WinForms 测试
```

x64 包包含下文列出的全部模型；为控制内存占用，x86 包仅包含 PP-OCRv6 tiny。

## 运行截图

Windows 7 SP1 下运行的 HTTP OCR 浏览器界面：

![Windows 7 SP1 下的 HTTP OCR 浏览器界面](images/screenshot-httpserver-win7.png)

Windows 7 SP1 下运行的 .NET Framework 3.5 WinForms 测试与测速程序：

![Windows 7 SP1 下的 WinForms OCR 测试程序](images/screenshot-winforms-win7.png)

## 为支持 Win7，我们具体做了什么

"支持 Win7"不是把工程里的目标系统改个名字就结束了。但凡有一处引入高版本依赖，程序就可能直接报错或闪退。

### 1. 从源码编译 OpenCV 5

原生引擎基于 OpenCV 5.0.0 源码构建，使用 Visual Studio 2019 的 MSVC v142 工具集。本公开仓库不包含 OpenCV 源码/静态库构建树，也不包含原生引擎的 C++ 源码。v142 满足 OpenCV 5 对 C++17 的要求，同时比 VS2022 的 v143 更适合作为 Win7 兼容目标。

### 2. 明确指定 Windows 7 子系统版本

C++ DLL 和 HTTP Server 均强制设置：

```
WINVER=0x0601
_WIN32_WINNT=0x0601
PE Subsystem Version = 6.01
```

6.01 即 Windows 7。在编译和链接阶段约束目标系统，避免无意调用高版本 Windows API。

### 3. 静态集成 OpenCV 和 C++ 运行时

OpenCV 5 模块使用静态库链接，MSVC 运行时使用 `/MT` 静态链接。最终 `lw.OpenCVDNN.PPOCR.dll` 的导入表只剩两个系统 DLL：

```
KERNEL32.dll
ole32.dll
```

不需要 `opencv_world500.dll`，不需要 `VCRUNTIME140.dll`。对工业现场来说，依赖越少，安装失败和版本冲突的概率就越低。

### 4. .NET Framework 3.5 C# 调用层

主测试程序明确面向 .NET Framework 3.5 x64，x86 发布包中也包含对应的
x86 构建。C# 通过 P/Invoke 调用稳定的 C ABI，不依赖 OpenCvSharp，不需要
NuGet 运行组件。模型路径和错误消息使用 UTF-16，OCR 结果使用 UTF-8 JSON——
中文目录和中文识别结果均可正确处理。

### 5. 移除 GPU 依赖

Win7 工控机的显卡型号、驱动版本和 CUDA 环境参差不齐，因此这一版只保留 CPU 推理。GPU 参数和相关代码分支已移除，不需要 CUDA、cuDNN、DirectML 或 OpenVINO 运行时。

### 6. 整体可复制的绿色工程

公开 SDK 将可构建的 HTTP 服务和示例源码，与可直接运行的二进制、模型、测试图片及接入文件放在同一目录。程序根据 EXE 所在目录寻找 `inference/` 和 `www/`，不依赖启动时的当前工作目录，放在中文路径下也能正常初始化。

> **注意：** 不同 Win7 工控机的补丁等级、CPU 指令集和系统组件可能不同。项目已按 Win7 SP1 x64 目标构建并控制依赖，正式部署前仍建议在客户的实际设备或同配置镜像中完成验收测试。

## 快速开始

### 1. 下载

从 [Releases](https://github.com/lxw112190/lw.OpenCVDNN.PPOCR/releases) 下载最新的 `win-x64` 或 `win-x86` 包。

### 2. 启动 HTTP 服务

仅本机使用时，建议显式绑定回环地址：

```bat
lw.OpenCVDNN.PPOCR.HttpServer.exe --host 127.0.0.1 --port 8080
```

浏览器打开 `http://127.0.0.1:8080/`，上传图片，点击**识别**。

`start_console.bat` 和 Windows 服务安装脚本使用默认的 `0.0.0.0` 绑定，
便于可信局域网客户端访问。

> **安全提示：** 服务默认监听 `0.0.0.0`，本身不提供鉴权或 TLS。仅本机
> 使用时请显式指定 `--host 127.0.0.1`；只应在可信局域网直接开放。需要跨
> 网络访问时，应放在带鉴权和 HTTPS 的反向代理之后，并配置相应防火墙规则。

### 3. Windows 服务

以管理员身份运行：

```bat
deploy\install_service.bat
```

卸载：

```bat
deploy\uninstall_service.bat
```

## API 接口

### HTTP 端点

```http
POST /api/ocr
Content-Type: application/json

{"imageBase64": "data:image/jpeg;base64,..."}
```

### curl 调用

```bash
curl -X POST "http://127.0.0.1:8080/api/ocr" \
  -H "Content-Type: application/json" \
  -d '{"imageBase64":"...图片Base64内容..."}'
```

### JavaScript（浏览器）

```js
const response = await fetch("http://127.0.0.1:8080/api/ocr", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ imageBase64 })
});
const result = await response.json();
console.log(result.elapsed_ms, result.results);
```

这段代码应从发布包自带页面或其他同源页面运行。浏览器跨域调用时，需要由
代理层提供合适的 CORS 策略。

### 返回格式

```json
{
  "code": 0,
  "msg": "ok",
  "elapsed_ms": 165.328,
  "results": [
    {
      "text": "纯臻营养护发素",
      "score": 1.0,
      "x1": 22, "y1": 32,
      "x2": 307, "y2": 32,
      "x3": 307, "y3": 75,
      "x4": 22, "y4": 75
    }
  ]
}
```

### C# (.NET Framework 3.5) 完整调用示例

<details>
<summary>展开完整 C# 示例</summary>

```csharp
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PpocrConfig
    {
        public int struct_size;
        [MarshalAs(UnmanagedType.LPWStr)] public string det_model_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string rec_model_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string rec_dict_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string cls_model_path;
        public int limit_side_len;
        public double det_db_thresh;
        public double det_db_box_thresh;
        public double det_db_unclip_ratio;
        public int use_dilation;
        public int use_angle_cls;
        public double cls_thresh;
        public int cls_batch_num;
        public int rec_batch_num;
        public int rec_img_h;
        public int rec_img_w;
        public int rec_predictor_num;
        public int cpu_threads;
    }

    private const string DllName = "lw.OpenCVDNN.PPOCR.dll";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ppocr_config_init(ref PpocrConfig config);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private static extern int ppocr_create_ex_w(ref PpocrConfig config,
        out IntPtr engine, StringBuilder error, int errorCapacity);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private static extern int ppocr_ocr_encoded(IntPtr engine,
        IntPtr imageBytes, int imageSize, out IntPtr utf8Json,
        out int jsonSize, StringBuilder error, int errorCapacity);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ppocr_free(IntPtr memory);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ppocr_destroy(IntPtr engine);

    static void Main(string[] args)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string modelDir = Path.Combine(baseDir, "inference");
        string imagePath = args.Length > 0
            ? Path.GetFullPath(args[0])
            : Path.Combine(baseDir, "test-images", "3.jpg");

        PpocrConfig config = new PpocrConfig();
        ppocr_config_init(ref config);
        config.det_model_path = Path.Combine(modelDir, "PP-OCRv6_tiny_det.onnx");
        config.rec_model_path = Path.Combine(modelDir, "PP-OCRv6_tiny_rec.onnx");
        config.rec_dict_path = Path.Combine(modelDir, "PP-OCRv6_tiny_rec_dict.txt");
        config.rec_predictor_num = 1;
        config.cpu_threads = 0;

        IntPtr engine = IntPtr.Zero;
        StringBuilder error = new StringBuilder(512);
        int code = ppocr_create_ex_w(ref config, out engine, error, error.Capacity);
        if (code != 0)
            throw new InvalidOperationException("初始化失败(" + code + "): " + error);

        try
        {
            byte[] image = File.ReadAllBytes(imagePath);
            GCHandle pinned = GCHandle.Alloc(image, GCHandleType.Pinned);
            IntPtr jsonPtr = IntPtr.Zero;
            try
            {
                int jsonSize;
                code = ppocr_ocr_encoded(engine, pinned.AddrOfPinnedObject(),
                    image.Length, out jsonPtr, out jsonSize, error, error.Capacity);
                if (code != 0)
                    throw new InvalidOperationException("识别失败(" + code + "): " + error);

                byte[] jsonBytes = new byte[jsonSize];
                Marshal.Copy(jsonPtr, jsonBytes, 0, jsonSize);
                Console.WriteLine(Encoding.UTF8.GetString(jsonBytes));
            }
            finally
            {
                if (jsonPtr != IntPtr.Zero) ppocr_free(jsonPtr);
                pinned.Free();
            }
        }
        finally
        {
            if (engine != IntPtr.Zero) ppocr_destroy(engine);
        }
    }
}
```

**两个关键点：** 调用程序位数必须与所选 DLL 一致（x64 包对应 x64 进程，
x86 包对应 x86 进程）；DLL 返回的 JSON 内存必须调用 `ppocr_free` 释放，
不能交给 C# GC 处理。

</details>

### C/C++

```cpp
#include "ppocr_api.h"

ppocr_config_w config{};
ppocr_config_init(&config);
config.det_model_path = L"inference/PP-OCRv6_tiny_det.onnx";
config.rec_model_path = L"inference/PP-OCRv6_tiny_rec.onnx";
config.rec_dict_path = L"inference/PP-OCRv6_tiny_rec_dict.txt";

wchar_t error[512]{};
ppocr_handle engine = nullptr;
if (ppocr_create_ex_w(&config, &engine, error, 512) == 0) {
    char* json = nullptr;
    int32_t json_size = 0;
    ppocr_ocr_encoded(engine, image_bytes, image_size, &json, &json_size, error, 512);
    // 使用 json（UTF-8 编码）
    ppocr_free(json);
    ppocr_destroy(engine);
}
```

Python ctypes、Delphi、Java JNA 等语言接入请参考 [SDK 接入指南](docs/SDK_INTEGRATION.md)。

## 为什么提供 C ABI

C++ 类接口容易受到编译器、运行库和 ABI 变化影响。发布版只暴露一组 `extern "C"` + `__cdecl` 函数。模型路径使用 UTF-16，识别结果统一返回 UTF-8 JSON。

因此除了 C#，C、C++、Python ctypes、Delphi、Java JNA 等语言也可以直接
对接，避免 C++ name mangling，并减少调用方对编译器和 C++ 运行时 ABI 的耦合。

## 模型

| 模型 | 类型 | 大小 | 发布包 | 说明 |
| --- | --- | --- | --- | --- |
| PP-OCRv6 tiny | 检测 / 识别 | ~1.8 MB / ~4.5 MB | x64、x86 | HTTP 服务默认的快速模型。 |
| PP-OCRv6 small | 检测 / 识别 | ~9.8 MB / ~21 MB | x64 | 容量更高的 V6 方案。 |
| PP-OCRv5 mobile | 检测 / 识别 / 方向分类 | ~4.8 MB / ~16.6 MB / ~1 MB | x64 | V5 mobile，可选方向分类。 |
| PP-OCRv5 server | 检测 / 识别 | ~88 MB / ~84 MB | x64 | 容量最大的 V5 方案。 |

模型精度取决于实际图片。应使用有代表性的现场数据逐一验证，不能简单认为
模型越大就一定越准确。

## 系统要求

- Windows 7 SP1 及以上（x64 或 x86）
- CPU 支持 SSE2（运行时派发 SSE4.1 / AVX / AVX2 以获得更好性能）
- .NET Framework 3.5（仅 WinForms 测试程序需要；DLL 和 HTTP 服务不依赖 .NET）
- 不需要安装 OpenCV、VC++ 运行库、Python 或 CUDA

## 目录结构

```text
lw.OpenCVDNN.PPOCR/
├── .gitattributes         文本/二进制与换行规则
├── README.md              # 英文说明
├── README_zh.md           # 中文说明（本文件）
├── LICENSE                MIT 许可证
├── DISTRIBUTION.md        源码/二进制分发范围
├── CHANGELOG.md           变更记录
├── THIRD_PARTY_NOTICES.md 第三方组件声明
├── licenses/              随仓库分发的第三方许可证
│
├── include/
│   └── ppocr_api.h        公共 C API 头文件
│
├── third_party/           共享第三方库
│   ├── cpp-base64/        Base64 编解码（zlib 风格许可）
│   └── spdlog/include/    spdlog 头文件（MIT）
│
├── server/                HTTP 服务——开源
│   ├── README.md
│   ├── CMakeLists.txt
│   ├── main.cpp
│   ├── src/json.cpp       nlohmann/json（MIT）
│   ├── third_party/httplib.h  cpp-httplib，Win7 兼容修改版（MIT）
│   ├── projects/*.vcxproj VS 2019 项目文件
│   ├── www/index.html     浏览器测试页面
│   └── deploy/            服务安装/卸载脚本
│
├── release/
│   ├── win-x64/           x64 发布包
│   └── win-x86/           x86 发布包
│
├── examples/
│   ├── csharp-console/    .NET Framework 3.5 控制台示例
│   └── csharp-winforms/   .NET Framework 3.5 WinForms 测试程序
│
└── docs/
    ├── DEPLOYMENT.md      x64 部署说明
    ├── DEPLOYMENT_X86.md  x86 部署说明
    └── SDK_INTEGRATION.md SDK 接入指南
```

## HTTP 服务命令行参数

| 参数 | 默认值 | 说明 |
| --- | --- | --- |
| `--host` | `0.0.0.0` | 绑定地址 |
| `--port` | `8080` | 监听端口 |
| `--det_model` | `inference/PP-OCRv6_tiny_det.onnx` | 检测模型路径 |
| `--rec_model` | `inference/PP-OCRv6_tiny_rec.onnx` | 识别模型路径 |
| `--rec_dict` | `inference/PP-OCRv6_tiny_rec_dict.txt` | 识别字典路径 |
| `--limit_side_len` | `960` | 检测最大边长 |
| `--det_db_thresh` | `0.3` | 检测阈值 |
| `--det_db_box_thresh` | `0.6` | 检测框阈值 |
| `--det_db_unclip_ratio` | `1.5` | 扩张比例 |
| `--use_dilation` | `false` | 对检测掩码执行膨胀 |
| `--cls_model` | `inference/PP-OCRv5_mobile_cls_onnx.onnx` | 方向分类模型路径 |
| `--use_angle_cls` | `false` | 启用方向分类 |
| `--cls_thresh` | `0.9` | 方向分类阈值 |
| `--cls_batch_num` | `1` | 方向分类批次大小 |
| `--rec_batch_num` | `8`（x86: 4） | 识别批次数 |
| `--rec_img_h` | `48` | 识别输入高度 |
| `--rec_img_w` | `320` | 识别输入宽度 |
| `--rec_predictor_num` | `4`（x86: 1） | 识别预测器数量 |
| `--cpu_threads` | `0`（x86: 2） | CPU 线程数（0=OpenCV 默认） |
| `--service` | _(关闭)_ | 以 Windows 服务方式运行（由 SCM 调用） |
| `--help`、`-h` | — | 显示命令行帮助 |

x64 的 HTTP 请求体上限为 50 MB，x86 为 10 MB。服务本身不提供鉴权或
TLS；绑定非回环地址前，请先阅读快速开始中的安全提示。

## 如何正确测试速度

OCR 性能不能只报一次"最快耗时"。首次推理通常包含缓存和内存准备，图片尺寸、文本框数量、CPU 线程数也会明显影响结果。建议按以下方法记录：

1. 固定 CPU 电源模式、模型、图片和检测参数。
2. 初始化模型后先**预热 5 次**（不计入统计）。
3. 连续识别 100 次，记录**平均值、P50、P95 和最大值**。
4. 同时记录初始化时间、图片尺寸、文本框数量和进程内存。
5. 分别测试 `cpu_threads=0、1、2、4`，不要只看单次最低值。
6. HTTP 场景还要分别测试单请求延迟和多客户端总吞吐。

### 性能记录模板

截图中的耗时只能证明功能运行，不能作为可迁移的性能基准。发布性能数字时，
应记录足够信息，保证结果可以复现：

| CPU / 内存 / 系统 | 架构 | 模型 | 图片尺寸 / 文本框数 | 次数 | 平均值 / P50 / P95 / 最大值 |
|---|---|---|---|---|---|
| （待填写） | x64 或 x86 | （待填写） | （待填写） | 100+ | （待填写） |

## 稳定性处理

本仓库不包含预编译原生引擎源码。以下内容是对其内部实现和作者已执行检查的
说明，使用方仍应在自己的目标系统上验证发布二进制：

- OCR 主对象改为智能指针——模型加载中途抛异常也能自动释放已创建的对象。
- Windows 模型文件句柄使用 RAII 封装——读取或内存分配失败时不会遗留句柄。
- 识别预测器使用自动租约——OpenCV 推理异常时仍会归还预测器池。
- 修复多路径轮廓展开可能使用错误长度而越界的问题。
- 过滤无效文字裁剪——避免空图进入识别网络。
- DLL 返回的 UTF-8 JSON 统一通过 `ppocr_free` 释放。
- C# 封装将识别与销毁串行化——避免原生句柄正在使用时被释放。
- `ocr_server.log` 按大小滚动，单文件 1 MB，最多 7 个文件；
  `ocr_daily.log` 按天滚动并保留 7 天。

更准确的表述是：项目已经对主要资源所有权和异常路径进行了专项审计。任何需要长期无人值守运行的工业项目，仍建议在目标设备上执行数小时或数天的循环压力测试，并观察句柄数、线程数和内存是否稳定。

## 常见问题

**Q: 应该选择 x64 还是 x86？**
除非宿主进程必须保持 32 位，否则推荐 x64。调用程序与原生 DLL 的位数必须
一致：x64 进程使用 x64 包，x86 进程使用 x86 包。.NET 项目不要保持含糊的
`Any CPU`。x86 包仅包含 PP-OCRv6 tiny，并使用更低内存的默认参数。

**Q: `cpu_threads=0` 是什么意思？**
0 表示保留 OpenCV 默认线程策略；正整数会调用 `cv::setNumThreads`。线程越多不一定越快，多请求并发时还可能产生 CPU 过度竞争，应在目标机器实测 1、2、4 和 0。

**Q: 模型放在中文目录会不会失败？**
不会。模型路径通过 UTF-16 传入 DLL，程序也以 EXE 目录解析相对路径，中文路径已通过验证。

**Q: 初始化失败先检查什么？**
1. 进程位数是否与所选 x64 或 x86 DLL 一致（任务管理器 → 详细信息 → 平台）。
2. DLL 是否与 EXE 同目录。
3. 模型和字典文件是否存在且未损坏。
4. 识别模型与字典是否配套（v5 ↔ v5 字典，v6 ↔ v6 字典）。
5. 查看界面初始化信息或 `logs/` 目录中的错误日志。

**Q: 8080 端口被占用怎么办？**
```bat
lw.OpenCVDNN.PPOCR.HttpServer.exe --port 18080
```

**Q: 能不能多线程调用？**
同一个 OCR 句柄的调用会在 DLL 中串行保护。需要并行吞吐时可以由上层建立多个独立实例，但每个实例都会加载模型并占用内存，必须根据 CPU 核心数和内存容量测试。

**Q: 支持哪些图片格式？**
编码图片接口由 OpenCV 解码，常见的 JPG、PNG、BMP 可以直接传入。HTTP 接口使用 Base64 上传。超大图片建议先缩放或裁剪 ROI。

**Q: 为什么钢印、反光文字效果不好？**
OCR 模型更擅长对比度清晰的印刷文字。钢印主要依赖微弱明暗和边缘变化，建议从侧向光源、固定相机、ROI 裁剪、CLAHE、高通和 Scharr 梯度增强入手，必要时采集现场数据微调模型。

## 适用边界

这套方案适合依赖受限的 CPU OCR，但并不打算覆盖所有场景。

**适合：**
- 固定工位的标签、包装、铭牌、票据和印刷文字。
- Windows x64 或 x86、离线部署和老 .NET 上位机。
- 中低并发、本地调用或局域网 HTTP 服务。

**需要额外评估：**
- 高反光、低对比钢印、严重透视、弯曲文字和手写体。
- 每秒大量并发请求或超高分辨率整图。
- 强制要求 x86 进程的历史软件（x86 包可用但吞吐受限）。
- 没有完成 Win7 SP1 补丁和硬件指令集核验的特殊设备。

主动说明边界不是削弱产品，反而能帮助项目在正确的环境中稳定落地。

## 上线前检查清单

- [ ] 目标系统为 Windows 7 SP1 或更高版本。
- [ ] 调用程序位数明确匹配所选 x64 或 x86 DLL。
- [ ] DLL、模型和字典已经复制完整。
- [ ] 识别模型与对应字典配套。
- [ ] 使用客户真实图片完成准确率验收。
- [ ] 对初始化、首次推理和预热后速度分别计时。
- [ ] 连续循环至少 1000 次，观察内存、句柄和线程数。
- [ ] HTTP 部署已确认端口、防火墙和局域网访问范围。
- [ ] Windows 服务使用有权限读取模型和写入日志的账户。
- [ ] 已保存可回退的旧版本程序和配置。

## 构建仓库中包含的源码

### lw.OpenCVDNN.PPOCR.dll（原生 OCR 引擎）

C++ DLL **不包含源码**，因此无法仅凭本仓库重新构建原生引擎。预编译二进制文件位于 `release/` 目录和 [Releases](https://github.com/lxw112190/lw.OpenCVDNN.PPOCR/releases) 页面。许可与分发范围见 [DISTRIBUTION.md](DISTRIBUTION.md)。

### HTTP 服务（开源）

HTTP 服务完整源码在 `server/` 目录下。它通过公开的 C ABI 链接预编译的 DLL。

**编译要求：** Visual Studio 2019 (v142)、Windows 10/11 SDK、CMake 3.20+。

```bat
cd server
mkdir build && cd build
cmake .. -G "Visual Studio 16 2019" -A x64
cmake --build . --config Release
```

CMake 默认值已经指向仓库内的 x64 SDK。构建 x86 时，请使用
[server/README.md](server/README.md) 中列出的 x86 SDK 路径。若直接使用
Visual Studio 项目，应将其放入“解决方案目录为仓库根目录”的解决方案中，
保证 `$(SolutionDir)` 可以正确解析。

### C# 示例

使用 Visual Studio 2019+，面向 .NET Framework 3.5 编译。不需要 NuGet 包。

## 第三方组件

详见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

| 组件 | 版本 | 许可证 | 用途 |
| --- | --- | --- | --- |
| OpenCV | 5.0.0 | Apache 2.0 | DLL（静态链接） |
| cpp-base64 | 2.rc.08 | zlib 风格许可 | HTTP 服务 |
| cpp-httplib | 0.48.0（Win7 兼容修改版） | MIT | HTTP 服务 |
| spdlog | 1.17.0 | MIT | HTTP 服务 |
| nlohmann/json | 2.0.0 | MIT | HTTP 服务 |

## 开源许可

项目自有源码及随仓库分发的原生二进制采用 MIT 许可证；原生引擎源码不包含在本仓库中。第三方代码和模型继续适用各自许可证。详见 [LICENSE](LICENSE)、[DISTRIBUTION.md](DISTRIBUTION.md)、[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) 和 [`licenses/`](licenses/)。

ONNX 模型源自 [PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR)（Apache License 2.0）。

## 支持作者 / Sponsor

如果这个项目对你有帮助，欢迎打赏支持作者。

![Sponsor](images/sponsor.jpg)
