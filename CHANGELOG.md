# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0.1] — 2026-07

### Fixed

- Prevented out-of-bounds writes in UTF-8/UTF-16 path conversion.
- Corrected x64/x86 HTTP server import-library paths.
- Preserved the default MSVC exception-unwinding flags in x86 CMake builds.
- Clarified the mixed source/binary distribution scope and bundled all required
  third-party license texts in the repository and release packages.

### Added

- Initial public release.
- PP-OCRv5 mobile model support (detection, recognition, angle classification).
- PP-OCRv5 server model support (detection, recognition).
- PP-OCRv6 tiny model support (detection, recognition).
- PP-OCRv6 small model support (detection, recognition).
- HTTP REST API server (`lw.OpenCVDNN.PPOCR.HttpServer.exe`) with `/api/ocr` endpoint.
- Built-in web UI with image preview and bounding box overlay.
- .NET Framework 3.5 WinForms test application with model selection and speed benchmarking.
- .NET Framework 3.5 console speed test example.
- C ABI DLL (`lw.OpenCVDNN.PPOCR.dll`) with `ppocr_create_ex_w`, `ppocr_ocr_encoded`, `ppocr_ocr_bgr` APIs.
- Windows service support via `--service` flag and `deploy/` batch scripts.
- x64 and x86 (32-bit) builds for legacy industrial systems.
- Windows 7 SP1 compatibility (`_WIN32_WINNT=0x0601`, static CRT).
- Static linking of OpenCV 5.0.0 and VC++ runtime — no external DLL dependencies beyond system DLLs.
- SDK with C header (`ppocr_api.h`), import library, and C# P/Invoke wrapper.
- SSE2 baseline CPU dispatch with SSE4.1/AVX/AVX2 runtime optimization.
