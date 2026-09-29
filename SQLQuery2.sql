-- Dữ liệu mẫu bảng giá (tùy chọn). Cũng có thể nhập qua trang Admin > Bảng giá.
INSERT INTO PriceTables (DeliveryMethod, MinWeight, MaxWeight, WeightRange, DistanceRange, Price)
VALUES
(N'Thường', 0,  5,  N'0 - 5 kg',   N'', 20000),
(N'Thường', 5,  20, N'5 - 20 kg',  N'', 40000),
(N'Thường', 20, 100, N'20 - 100 kg', N'', 90000),
(N'Nhanh',  0,  5,  N'0 - 5 kg',   N'', 35000),
(N'Nhanh',  5,  20, N'5 - 20 kg',  N'', 70000),
(N'Nhanh',  20, 100, N'20 - 100 kg', N'', 150000);
