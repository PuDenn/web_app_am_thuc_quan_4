# 🍜 Ẩm Thực Quận 4 — .NET MAUI Native

Ứng dụng tour ẩm thực Quận 4 thuần MAUI Native (không WebView).

## Cấu trúc

```
AmThucQuan4.Native/
├── Models/
│   └── PoiModel.cs              ← Data model (Name, Lat/Lng, AudioPath...)
├── Services/
│   ├── PoiService.cs            ← Data 5 POI Đoàn Văn Bơ
│   ├── MapService.cs            ← OSRM routing + Nominatim geocoding
│   └── AudioService.cs          ← Plugin.Maui.Audio + TTS fallback
├── ViewModels/
│   └── MainViewModel.cs         ← MVVM logic đầy đủ
├── Views/
│   ├── MainPage.xaml            ← UI: Map + BottomSheet + DetailPanel + Search
│   └── MainPage.xaml.cs         ← Mapsui init, pin/route/user marker
├── Converters/
│   └── Converters.cs            ← NullToBool, BoolToAudioText, InvertBool
└── Platforms/Android/
    ├── AndroidManifest.xml
    ├── MainActivity.cs
    └── MainApplication.cs
```

## Tính năng

| Tính năng | Chi tiết |
|---|---|
| 🗺️ Bản đồ | Mapsui + OpenStreetMap tile (miễn phí, không cần API key) |
| 📍 GPS | Geolocation.GetLocationAsync, cập nhật user marker |
| 🛣️ Dẫn đường | OSRM API → vẽ polyline cam trên map |
| 🔍 Tìm địa chỉ | Nominatim geocoding → fly đến vị trí |
| 📋 POI List | CollectionView, sort theo khoảng cách Haversine |
| 🎵 Audio | Plugin.Maui.Audio + TTS fallback (vi-VN) |
| 🏗️ Kiến trúc | MVVM với CommunityToolkit.Mvvm |

## Cách chạy

1. Tải về: `scp puden@<IP>:/home/puden/projects/AmThucQuan4.Native.tar.gz .`
2. Giải nén, mở `AmThucQuan4.Native.sln` trong Visual Studio 2022
3. Chọn Android Emulator → F5

## Test GPS trên Emulator

Mở Extended Controls (⋮) → Location:
| POI | Lat | Lng |
|---|---|---|
| Cơm Tấm Bà Út | 10.7573 | 106.7000 |
| Bánh Mì Huỳnh Hoa | 10.7575 | 106.7012 |
| Ốc Đào | 10.7577 | 106.7021 |
| Trà Sữa Phúc Long | 10.7580 | 106.7033 |
| Phở 24 | 10.7583 | 106.7048 |
