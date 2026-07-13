# x86绿色部署说明

版本：1.0.0.1  
作者：天天代码码天天，QQ：819069052

## 系统要求

- Windows 7 SP1或更高版本，可以是32位或64位Windows。
- 调用进程必须为x86，不能从x64进程加载本DLL。
- CPU必须支持SSE2。
- .NET Framework 3.5仅窗体测试程序需要；原生DLL和HTTP服务不依赖.NET。
- 不需要安装OpenCV、OpenCvSharp、VC++运行库、Python或CUDA。

## 默认轻量配置

```text
模型：PP-OCRv6 tiny
HTTP工作线程：2
识别预测器：1
识别批次：4
OpenCV CPU线程：2
HTTP请求上限：10MB
```

## 窗体测试

双击 `start_winforms_test.bat`。界面应显示：

```text
进程架构：x86（低内存模式）
```

选择图片后依次点击“初始化”和“识别/测速”。

## HTTP服务

双击 `start_console.bat`，浏览器访问：

```text
http://127.0.0.1:8080/
```

启动日志应包含：

```text
process_arch: 32-bit, HTTP workers: 2
mode: x86 low-memory / low-concurrency
```

## Windows服务

以管理员身份运行：

```text
deploy/install_service.bat
```

卸载服务：

```text
deploy/uninstall_service.bat
```

## 内存建议

x86进程地址空间有限。不要增加识别预测器数量，不建议同时创建多个OCR实例。正式上线前应在目标工控机连续识别至少1000次，并观察内存、句柄和线程数。

二进制已启用大地址支持；在64位Windows上运行时可获得更大的32位进程地址空间，但这不能代替实际设备的长期压力测试。
