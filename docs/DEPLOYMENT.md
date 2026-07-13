# 绿色部署说明

版本：1.0.0.1  
作者：天天代码码天天,QQ:819069052

## 文件

```text
lw.OpenCVDNN.PPOCR.dll                 OCR动态库，静态包含OpenCV 5
lw.OpenCVDNN.PPOCR.HttpServer.exe      HTTP服务
lw.OpenCVDNN.PPOCR.Win7.Test.exe       .NET Framework 3.5窗体测试程序
inference/                             ONNX模型和字典
test-images/                           测试图片
www/                                   HTTP测试页面
deploy/                                Windows服务安装和卸载脚本
```

## 窗体测试

双击：

```text
lw.OpenCVDNN.PPOCR.Win7.Test.exe
```

选择模型，点击“初始化”，初始化信息会显示在右侧。选择图片后点击“识别/测速”。

## HTTP服务

双击 `start_console.bat`，或者执行：

```bat
lw.OpenCVDNN.PPOCR.HttpServer.exe --port 8080
```

浏览器访问 `http://127.0.0.1:8080/`。

安装Windows服务时，以管理员身份运行：

```text
deploy/install_service.bat
```

卸载服务：

```text
deploy/uninstall_service.bat
```

## 系统要求

- Windows 7 SP1 x64及更高版本
- .NET Framework 3.5，仅窗体程序需要；HTTP服务和原生DLL不需要.NET
- 支持SSE2的x64 CPU

不需要安装 OpenCV、OpenCvSharp、VC++运行库或其他NuGet组件。

