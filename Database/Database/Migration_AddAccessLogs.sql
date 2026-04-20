USE AmThucQuan4DB;
GO

-- Thêm bảng AccessLogs để tracking sự kiện từ App và Web PWA
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AccessLogs')
BEGIN
    CREATE TABLE AccessLogs (
        Id        INT PRIMARY KEY IDENTITY(1,1),
        PoiId     INT NOT NULL,
        Type      NVARCHAR(20)  NOT NULL, -- 'view' | 'audio_play' | 'qr_scan'
        Source    NVARCHAR(10)  NOT NULL, -- 'app' | 'web'
        Timestamp DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Đã tạo bảng AccessLogs';
END
ELSE
    PRINT 'Bảng AccessLogs đã tồn tại';
GO

-- Seed vài dữ liệu mẫu để Dashboard có gì hiển thị
INSERT INTO AccessLogs (PoiId, Type, Source)
SELECT Id, 'view',      'app' FROM POIs
UNION ALL
SELECT Id, 'qr_scan',   'web' FROM POIs
UNION ALL
SELECT Id, 'audio_play','web' FROM POIs WHERE Id IN (1,2,3);
GO

PRINT 'Migration hoàn tất!';
