# 🍜 Ẩm Thực Quận 4 — Mobile App

Ứng dụng hướng dẫn ẩm thực tuyến đường Đoàn Văn Bơ, Quận 4, TP.HCM.

## Cấu trúc dự án

```
AmThucQuan4/
├── AmThucQuan4.Native/     # .NET MAUI Android App
├── AmThucQuan4.API/        # ASP.NET Core Web API
├── Database/               # Setup Database
└── AmThucQuan4.sln         # Visual Studio Solution
```

## Tính năng

### 📱 MAUI App (`AmThucQuan4.Native`)
- Bản đồ OpenStreetMap native (Mapsui)
- 5 POI quán ăn trên tuyến Đoàn Văn Bơ
- Chọn vị trí thủ công trên bản đồ (📌)
- Dẫn đường từ vị trí đến quán (OSRM)
- Phát thuyết minh TTS 5 ngôn ngữ (VI/EN/FR/ZH/JA)
- Đăng nhập / Đăng ký tài khoản
- Admin Panel: thêm/sửa/xóa quán, quản lý user

### 🌐 API (`AmThucQuan4.API`)
- ASP.NET Core 9
- JWT Authentication
- SQL Server (AmThucQuan4DB)
- CRUD: POIs, Users, Tours

## Yêu cầu

- .NET 9 SDK + MAUI workload
- SQL Server (instance: `DESKTOP-EE3NTUQ`)
- Android Emulator (Pixel 6 API 33) hoặc thiết bị thật

## Cài đặt

### 1. Database
```sql
-- Chạy script tạo DB trong SQL Server Management Studio
-- Hoặc dùng EF migrations (coming soon)
```

### 2. API
```bash
cd AmThucQuan4.API
dotnet run
# API chạy tại http://localhost:5000
```

### 3. MAUI App
```bash
cd AmThucQuan4.Native
dotnet build
# Deploy lên emulator / thiết bị từ Visual Studio
```

## Tài khoản mặc định

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | Admin |

## Công nghệ

| Layer | Technology |
|-------|-----------|
| Mobile | .NET MAUI 9, Mapsui 5 |
| API | ASP.NET Core 9, EF Core |
| Database | SQL Server 2019+ |
| Auth | JWT Bearer Token |
| Map | OpenStreetMap + OSRM |
| TTS | .NET MAUI TextToSpeech |
