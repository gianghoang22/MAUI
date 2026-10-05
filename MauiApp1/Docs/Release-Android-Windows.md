# VocabMate release checklist: Android and Windows

Phạm vi tài liệu này là Android và Windows. iOS/MacCatalyst không thuộc target hiện tại.

## Trước khi phát hành

- Đổi `ApplicationId` trong `MauiApp1/MauiApp1.csproj` thành định danh duy nhất do sản phẩm sở hữu.
- Chốt `ApplicationDisplayVersion` và tăng `ApplicationVersion` cho mỗi bản Android.
- Thay icon/splash thương hiệu nếu cần; kiểm tra cả launcher icon và Windows tile.
- Sao lưu `vocabmate.json` trước khi cài bản có application ID mới. Android coi application ID mới là ứng dụng khác và không tự migrate app data.

## Kiểm tra bắt buộc

```powershell
dotnet test MauiApp1.Tests/MauiApp1.Tests.csproj -c Release
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-android
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-windows10.0.19041.0
```

Sau khi build, chạy trên emulator/device thật để kiểm tra import file, Text-to-Speech, resume sau background, keyboard, font scale, theme và TalkBack/Narrator.

## Android signing

Không commit keystore, password hoặc service account. Cấu hình signing qua secret của CI hoặc user secrets. Kiểm tra package bằng bundle/APK release trên ít nhất một thiết bị API thấp hơn và API hiện tại.

## Windows distribution

Project hiện dùng `WindowsPackageType=None`, phù hợp chạy unpackaged trên Windows. Nếu phát hành MSIX/Store, cần chốt Publisher/Package Identity, certificate, manifest logo và quy trình ký riêng; không dùng các placeholder trong `Platforms/Windows/Package.appxmanifest`.

## CI

`.github/workflows/validate.yml` chỉ restore, chạy test và build Android/Windows Release. Signing và upload artifact chưa được tự động hóa vì cần certificate và thông tin tài khoản phát hành.
