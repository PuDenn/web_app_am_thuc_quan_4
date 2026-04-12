-- ============================================================
--  ẨM THỰC QUẬN 4 — Database Setup Script
--  Chạy script này trong SQL Server Management Studio (SSMS)
--  Chỉ cần chạy 1 lần duy nhất trên máy mới
-- ============================================================

USE master;
GO

-- Tạo database nếu chưa có
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AmThucQuan4DB')
BEGIN
    CREATE DATABASE AmThucQuan4DB;
    PRINT 'Database AmThucQuan4DB đã được tạo.';
END
ELSE
BEGIN
    PRINT 'Database AmThucQuan4DB đã tồn tại, bỏ qua bước tạo.';
END
GO

USE AmThucQuan4DB;
GO

-- ============================================================
-- BẢNG USERS
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE Users (
        Id          INT            PRIMARY KEY IDENTITY(1,1),
        Username    NVARCHAR(50)   NOT NULL UNIQUE,
        Email       NVARCHAR(100)  NOT NULL UNIQUE,
        PasswordHash NVARCHAR(300) NOT NULL,
        Role        NVARCHAR(20)   NOT NULL DEFAULT 'User',  -- 'User' | 'Admin'
        IsActive    BIT            NOT NULL DEFAULT 1,
        CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
        LastLoginAt DATETIME       NULL
    );
    PRINT 'Bảng Users đã được tạo.';
END
GO

-- ============================================================
-- BẢNG POIs (Điểm du lịch / quán ăn)
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'POIs')
BEGIN
    CREATE TABLE POIs (
        Id           INT            PRIMARY KEY IDENTITY(1,1),
        Name         NVARCHAR(100)  NOT NULL,
        Category     NVARCHAR(50)   NULL,
        CategoryIcon NVARCHAR(10)   NULL DEFAULT N'🍽️',
        Description  NVARCHAR(500)  NULL,
        Address      NVARCHAR(200)  NULL,
        Hours        NVARCHAR(50)   NULL,
        PriceRange   NVARCHAR(50)   NULL,
        ImageUrl     NVARCHAR(500)  NULL,
        AudioPath    NVARCHAR(200)  NULL,
        AudioScript  NVARCHAR(2000) NULL,
        Latitude     FLOAT          NOT NULL DEFAULT 0,
        Longitude    FLOAT          NOT NULL DEFAULT 0,
        IsVisible    BIT            NOT NULL DEFAULT 1,
        CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Bảng POIs đã được tạo.';
END
GO

-- ============================================================
-- BẢNG TOURS
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tours')
BEGIN
    CREATE TABLE Tours (
        Id          INT           PRIMARY KEY IDENTITY(1,1),
        Name        NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Bảng Tours đã được tạo.';
END
GO

-- ============================================================
-- BẢNG TOUR_POIS (Quan hệ Tour — POI)
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TourPOIs')
BEGIN
    CREATE TABLE TourPOIs (
        Id       INT PRIMARY KEY IDENTITY(1,1),
        TourId   INT NOT NULL REFERENCES Tours(Id) ON DELETE CASCADE,
        PoiId    INT NOT NULL REFERENCES POIs(Id)  ON DELETE CASCADE,
        OrderNum INT NOT NULL DEFAULT 0
    );
    PRINT 'Bảng TourPOIs đã được tạo.';
END
GO

-- ============================================================
-- SEED DATA — 5 POI Tuyến Đoàn Văn Bơ, Quận 4
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM POIs WHERE Name = N'Cơm Tấm Bà Út')
BEGIN
    INSERT INTO POIs (Name, Category, CategoryIcon, Description, Address, Hours, PriceRange,
                      ImageUrl, AudioScript, Latitude, Longitude, IsVisible)
    VALUES
    -- POI 1
    (N'Cơm Tấm Bà Út',
     N'Cơm Tấm', N'🍚',
     N'Quán cơm tấm lâu đời nằm trên đường Hoàng Diệu, nổi tiếng với sườn nướng than hoa thơm lừng và bì trộn chuẩn vị Sài Gòn xưa.',
     N'32 Hoàng Diệu, Phường 10, Quận 4, TP.HCM',
     N'06:00 – 14:00', N'35k – 65k',
     N'https://images.unsplash.com/photo-1630984931587-5db2946e5c40?w=600&q=80',
     N'Chào mừng bạn đến Cơm Tấm Bà Út — một trong những quán cơm tấm lâu đời và nổi tiếng nhất Quận 4. Quán tọa lạc tại số 32 đường Hoàng Diệu, mở cửa từ sáu giờ sáng đến hai giờ chiều. Điểm đặc biệt là sườn nướng than hoa — thịt mềm, thơm khói, được ướp theo công thức gia truyền hơn 30 năm. Giá dao động từ 35 đến 65 nghìn đồng.',
     10.7573, 106.7000, 1),

    -- POI 2
    (N'Bánh Mì Huỳnh Hoa',
     N'Bánh Mì', N'🥖',
     N'Tiệm bánh mì nổi danh khắp Sài Gòn với ổ bánh giòn rụm, nhân đầy ắp chả lụa, thịt nguội, pa-tê béo ngậy.',
     N'26 Đoàn Văn Bơ, Phường 13, Quận 4, TP.HCM',
     N'06:00 – 14:00', N'30k – 45k',
     N'https://images.unsplash.com/photo-1509722747041-616f39b57569?w=600&q=80',
     N'Bạn đang đứng trước Bánh Mì Huỳnh Hoa — tiệm bánh mì nổi tiếng bậc nhất Quận 4. Địa chỉ tại số 26 Đoàn Văn Bơ, mở từ sáu giờ sáng. Ổ bánh mì ở đây giòn rụm, nhân cực kỳ đầy đặn: chả lụa thái dày, pa-tê béo ngậy. Giá từ 30 đến 45 nghìn.',
     10.7575, 106.7012, 1),

    -- POI 3
    (N'Ốc Đào',
     N'Ốc', N'🐚',
     N'Quán ốc vỉa hè đặc trưng Sài Gòn, nức tiếng với ốc len xào dừa béo ngậy, nghêu hấp sả thơm lừng.',
     N'5 Đoàn Văn Bơ, Phường 12, Quận 4, TP.HCM',
     N'17:00 – 23:00', N'50k – 150k',
     N'https://images.unsplash.com/photo-1559410545-0bdcd187e0a6?w=600&q=80',
     N'Chào mừng bạn đến Ốc Đào — thiên đường ốc vỉa hè của Quận 4! Quán nằm tại số 5 Đoàn Văn Bơ, phục vụ từ năm giờ chiều đến mười một giờ đêm. Món được yêu thích nhất là ốc len xào dừa và nghêu hấp sả. Giá từ 50 đến 150 nghìn tùy món.',
     10.7577, 106.7021, 1),

    -- POI 4
    (N'Trà Sữa Phúc Long',
     N'Trà Sữa', N'🧋',
     N'Thương hiệu trà sữa Việt nổi tiếng với trà ô long, trà xanh và cà phê đặc trưng. Hơn 60 năm lịch sử.',
     N'10 Đoàn Văn Bơ, Phường 12, Quận 4, TP.HCM',
     N'07:00 – 22:00', N'29k – 65k',
     N'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80',
     N'Bạn đang đến Phúc Long — thương hiệu đồ uống Việt Nam có lịch sử hơn 60 năm. Chi nhánh này tọa lạc tại số 10 Đoàn Văn Bơ. Trà ô long sữa và cà phê phin truyền thống là những thức uống signature. Giá từ 29 đến 65 nghìn đồng.',
     10.7580, 106.7033, 1),

    -- POI 5
    (N'Phở 24',
     N'Phở', N'🍜',
     N'Chuỗi phở Việt chuẩn vị với nước dùng trong vắt, ninh từ xương bò tươi 8 tiếng. Thịt bò tái mềm, bánh phở dai.',
     N'8 Nguyễn Tất Thành, Phường 13, Quận 4, TP.HCM',
     N'06:00 – 10:30', N'45k – 75k',
     N'https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=600&q=80',
     N'Chào mừng bạn đến Phở 24 tại số 8 Nguyễn Tất Thành, Quận 4. Nước dùng phở được ninh từ xương ống bò tươi trong tám tiếng đồng hồ — trong vắt, ngọt thanh. Quán chỉ mở đến mười giờ rưỡi sáng. Giá từ 45 đến 75 nghìn đồng.',
     10.7583, 106.7048, 1);

    PRINT 'Đã thêm 5 POI mẫu.';
END
GO

-- ============================================================
-- SEED DATA — Tour mẫu
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Tours WHERE Name = N'Tour Ẩm Thực Đoàn Văn Bơ')
BEGIN
    INSERT INTO Tours (Name, Description)
    VALUES (N'Tour Ẩm Thực Đoàn Văn Bơ',
            N'Khám phá 5 quán ăn nổi tiếng trên tuyến đường Đoàn Văn Bơ, Quận 4, TP.HCM');

    INSERT INTO TourPOIs (TourId, PoiId, OrderNum)
    SELECT t.Id, p.Id,
           ROW_NUMBER() OVER (ORDER BY p.Id)
    FROM Tours t, POIs p
    WHERE t.Name = N'Tour Ẩm Thực Đoàn Văn Bơ';

    PRINT 'Đã tạo Tour mẫu.';
END
GO

-- ============================================================
-- SEED DATA — Admin Account
-- QUAN TRỌNG: Sau khi chạy script này, vào Swagger gọi:
--   POST http://localhost:5000/api/setup/init
-- để tạo password hash đúng cho tài khoản Admin
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    -- Tạm thêm admin với placeholder — sẽ được fix bởi /api/setup/init
    INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive)
    VALUES ('admin', 'admin@amthucquan4.com',
            '$PLACEHOLDER$',  -- sẽ được thay bằng BCrypt hash qua /api/setup/init
            'Admin', 1);
    PRINT 'Tài khoản admin tạm thời đã được tạo.';
    PRINT '>>> Hãy gọi POST http://localhost:5000/api/setup/init để kích hoạt!';
END
GO

-- ============================================================
-- KIỂM TRA KẾT QUẢ
-- ============================================================
PRINT '=== KIỂM TRA DỮ LIỆU ===';
SELECT 'Users'  AS [Bảng], COUNT(*) AS [Số dòng] FROM Users  UNION ALL
SELECT 'POIs'   AS [Bảng], COUNT(*) AS [Số dòng] FROM POIs   UNION ALL
SELECT 'Tours'  AS [Bảng], COUNT(*) AS [Số dòng] FROM Tours  UNION ALL
SELECT 'TourPOIs' AS [Bảng], COUNT(*) AS [Số dòng] FROM TourPOIs;
GO

PRINT '';
PRINT '✅ Setup hoàn tất!';
PRINT '   1. Chạy API: cd AmThucQuan4.API && dotnet run';
PRINT '   2. Mở http://localhost:5000/swagger';
PRINT '   3. Gọi POST /api/setup/init để kích hoạt tài khoản admin';
PRINT '   4. Đăng nhập: admin / Admin@123';
GO
