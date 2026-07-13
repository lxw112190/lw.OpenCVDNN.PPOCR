# Distribution and Source Availability

## English

This repository is a mixed source/binary SDK distribution.

- Project-authored source under `server/`, `examples/`, and `include/` is
  provided under the MIT License in `LICENSE`.
- Pre-built `lw.OpenCVDNN.PPOCR.dll` binaries are provided under the same MIT
  License for the project-authored portions. Their C++ source code is not
  included, so the native OCR engine is not an open-source source distribution
  and cannot be rebuilt from this repository alone.
- Third-party source, statically linked components, and ONNX models remain
  subject to their respective licenses. See `THIRD_PARTY_NOTICES.md` and
  `licenses/`.
- The HTTP server, public C ABI header, and C# examples can be inspected and
  rebuilt from the source included here. Building the HTTP server requires the
  pre-built native DLL and import library under `release/<arch>/sdk/native/`.

## 中文

本仓库是“源码 + 预编译二进制”的混合 SDK 发布包。

- `server/`、`examples/`、`include/` 中由本项目编写的源码采用根目录
  `LICENSE` 中的 MIT 许可证。
- 预编译的 `lw.OpenCVDNN.PPOCR.dll` 中由本项目编写的部分同样按 MIT
  许可证分发，但仓库不包含其 C++ 源码，因此原生 OCR 引擎本身不是一份
  可从源码重建的开源源码发行版，也无法仅凭本仓库重新构建。
- 第三方源码、静态链接组件和 ONNX 模型继续适用各自许可证，详见
  `THIRD_PARTY_NOTICES.md` 和 `licenses/`。
- HTTP 服务、公共 C ABI 头文件和 C# 示例均包含可查看、可构建的源码。
  构建 HTTP 服务时需要 `release/<架构>/sdk/native/` 中的预编译 DLL 和导入库。
