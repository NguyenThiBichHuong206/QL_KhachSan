-- 1. Tạo Database
CREATE DATABASE QuanLyKhachSan;
GO
USE QuanLyKhachSan;
GO

-- 2. Tạo bảng LoaiPhong
CREATE TABLE LoaiPhong (
    MaLoai INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(50) NOT NULL,
    DonGia DECIMAL(18, 2) NOT NULL
);

-- 3. Tạo bảng Phong
CREATE TABLE Phong (
    MaPhong INT PRIMARY KEY IDENTITY(1,1),
    TenPhong NVARCHAR(50) NOT NULL,
    MaLoai INT,
    TrangThai INT DEFAULT 0, -- 0: Trống, 1: Đang thuê
    CONSTRAINT FK_Phong_LoaiPhong FOREIGN KEY (MaLoai) REFERENCES LoaiPhong(MaLoai)
);

-- 4. Tạo bảng NhanVien (Bao gồm TaiKhoan)
CREATE TABLE NhanVien (
    MaNV INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    TaiKhoan VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    VaiTro INT DEFAULT 0 -- 1: Admin, 0: Lễ tân
);

-- 5. Tạo bảng KhachHang
CREATE TABLE KhachHang (
    MaKH INT PRIMARY KEY IDENTITY(1,1),
    CCCD VARCHAR(15) UNIQUE NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15)
);

-- 6. Tạo bảng DichVu
CREATE TABLE DichVu (
    MaDV INT PRIMARY KEY IDENTITY(1,1),
    TenDV NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18, 2) NOT NULL
);

-- 7. Tạo bảng HoaDon
CREATE TABLE HoaDon (
    MaHD INT PRIMARY KEY IDENTITY(1,1),
    MaPhong INT,
    MaKH INT,
    MaNV INT,
    NgayCheckIn DATETIME DEFAULT GETDATE(),
    NgayCheckOut DATETIME,
    TienPhong DECIMAL(18, 2) DEFAULT 0,
    TienDichVu DECIMAL(18, 2) DEFAULT 0,
    TongTien DECIMAL(18, 2) DEFAULT 0,
    TrangThaiThanhToan INT DEFAULT 0, -- 0: Chưa thanh toán, 1: Đã thanh toán
    CONSTRAINT FK_HoaDon_Phong FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong),
    CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

-- 8. Tạo bảng ChiTietHD (Để quản lý nhiều dịch vụ trên 1 hóa đơn)
CREATE TABLE ChiTietHD (
    MaCTHD INT PRIMARY KEY IDENTITY(1,1),
    MaHD INT,
    MaDV INT,
    SoLuong INT DEFAULT 1,
    DonGia DECIMAL(18, 2),
    CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD),
    CONSTRAINT FK_CTHD_DichVu FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV)
);
-- 9. Tạo bảng NhaCungCap
CREATE TABLE NhaCungCap (
    MaNCC INT PRIMARY KEY IDENTITY(1,1),
    TenNCC NVARCHAR(200) NOT NULL,
    DiaChi NVARCHAR(MAX),
    SDT VARCHAR(15)
);

-- 10. Tạo bảng HangHoa (Để quản lý kho và tồn kho)
CREATE TABLE HangHoa (
    MaHang INT PRIMARY KEY IDENTITY(1,1),
    TenHang NVARCHAR(100) NOT NULL,
    DonViTinh NVARCHAR(50),
    SoLuongTon INT DEFAULT 0, -- Rất quan trọng để Diễm làm CLO2 (1.0đ)
    GiaNhap DECIMAL(18,2)
);

-- 11. Tạo bảng hệ thống Phiếu Nhập Kho
CREATE TABLE PhieuNhapKho (
    MaPhieuNhap INT PRIMARY KEY IDENTITY(1,1),
    NgayNhap DATETIME DEFAULT GETDATE(),
    MaNCC INT FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV),
    TongTien DECIMAL(18,2) DEFAULT 0
);

-- 12. Tạo bảng hệ thống Chi tiết nhập Kho
CREATE TABLE ChiTietNhapKho (
    MaPhieuNhap INT FOREIGN KEY REFERENCES PhieuNhapKho(MaPhieuNhap),
    MaHang INT FOREIGN KEY REFERENCES HangHoa(MaHang),
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaPhieuNhap, MaHang)
);

-- 13. cột HinhAnh vào bảng Phong để Triết thực hiện CLO1
ALTER TABLE Phong ADD HinhAnh NVARCHAR(MAX);

-- =============================================
-- NHẬP DỮ LIỆU MẪU (5-10 dòng mỗi bảng)
-- =============================================

-- Dữ liệu LoaiPhong
INSERT INTO LoaiPhong (TenLoai, DonGia) VALUES 
(N'Phòng Đơn Standard', 300000),
(N'Phòng Đôi Standard', 500000),
(N'Phòng Đơn VIP', 600000),
(N'Phòng Đôi VIP', 1000000),
(N'Phòng Gia Đình', 1500000);

-- Dữ liệu Phong
INSERT INTO Phong (TenPhong, MaLoai, TrangThai) VALUES 
(N'P101', 1, 0), (N'P102', 1, 1), 
(N'P201', 2, 0), (N'P202', 2, 1),
(N'P301', 3, 0), (N'P401', 4, 1);

-- Dữ liệu NhanVien
INSERT INTO NhanVien (HoTen, TaiKhoan, MatKhau, VaiTro) VALUES 
(N'Nguyễn Văn Admin', 'admin', '123', 1),
(N'Lê Thị Lễ Tân', 'reception01', '123', 0),
(N'Trần Văn Tiếp Tân', 'reception02', '123', 0);

-- Dữ liệu KhachHang
INSERT INTO KhachHang (CCCD, HoTen, SDT) VALUES 
('079123456789', N'Nguyễn Anh Tuấn', '0901234567'),
('079987654321', N'Lê Thị Hồng', '0912345678'),
('080111222333', N'Phạm Minh Hoàng', '0988887777'),
('082444555666', N'Đặng Thu Thảo', '0977665544');

-- Dữ liệu DichVu
INSERT INTO DichVu (TenDV, DonGia) VALUES 
(N'Nước suối', 15000), (N'Mì ly', 20000), 
(N'Giặt ủi', 50000), (N'Thuê xe máy', 150000),
(N'Ăn sáng tại phòng', 100000);

-- Dữ liệu HoaDon (Mẫu hóa đơn chưa thanh toán)
INSERT INTO HoaDon (MaPhong, MaKH, MaNV, NgayCheckIn, TrangThaiThanhToan) VALUES 
(2, 1, 2, '2026-05-01 14:00:00', 0),
(4, 2, 2, '2026-05-02 10:00:00', 0);

-- Dữ liệu ChiTietHD
INSERT INTO ChiTietHD (MaHD, MaDV, SoLuong, DonGia) VALUES 
(1, 1, 2, 15000), (1, 3, 1, 50000),
(2, 4, 1, 150000);

-- Dữ liệu mẫu cho Nhà Cung Cấp
INSERT INTO NhaCungCap (TenNCC, DiaChi, SDT) VALUES 
(N'Công ty Thực phẩm sạch', N'Hồ Chí Minh', '0123456789'),
(N'Nội thất Khách sạn Kim khí', N'Hà Nội', '0987654321');

-- Dữ liệu mẫu cho Hàng Hóa (Tồn kho)
INSERT INTO HangHoa (TenHang, DonViTinh, SoLuongTon, GiaNhap) VALUES 
(N'Bàn chải đánh răng', N'Cái', 100, 5000),
(N'Xà phòng mini', N'Cục', 200, 3000),
(N'Nước suối Aquafina', N'Chai', 50, 7000);
