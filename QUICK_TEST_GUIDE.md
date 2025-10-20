# ⚡ QUICK TEST GUIDE - 5 phút

## 🎯 Test nhanh các thay đổi trong 5 phút

### BƯỚC 1: Build (30 giây)
```bash
# Vào thư mục project
cd /path/to/BookStoreMWC

# Build
dotnet build
```

**✅ Mong đợi:** `Build succeeded. 0 Error(s)`

---

### BƯỚC 2: Chạy App (30 giây)
```bash
dotnet run
# HOẶC nhấn F5 trong Visual Studio
```

**✅ Kiểm tra:**
- ❌ KHÔNG còn thấy: `==================== DEBUG INFO ====================`
- ✅ Thấy: `Now listening on: https://localhost:7001`

---

### BƯỚC 3: Test Admin Login (1 phút)
1. Mở trình duyệt: `https://localhost:7001`
2. Click **Đăng nhập**
3. Nhập:
   - Email: `admin@bookstore.com`
   - Password: `Admin123!`
4. Click Đăng nhập

**✅ Mong đợi:** Login thành công, thấy menu Admin

---

### BƯỚC 4: Test Giỏ Hàng (2 phút)
1. **Logout** (test giỏ hàng guest)
2. Browse books, click **"Thêm vào giỏ hàng"** cho 2-3 cuốn
3. Click vào icon giỏ hàng
4. Thay đổi số lượng
5. Xóa 1 item

**✅ Mong đợi:**
- Thêm/xóa/update NHANH, không lag
- Số lượng và giá cập nhật đúng
- Không có errors

---

### BƯỚC 5: Verify Configuration (1 phút)

**Check appsettings.json có đúng không:**

```json
{
  "AdminAccount": {
    "Email": "admin@bookstore.com",
    "Password": "Admin123!",
    "Name": "Quản trị viên"
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "https://localhost:5001",
      "https://localhost:7001"
    ]
  }
}
```

---

## ✅ CHECKLIST 5 PHÚT

```
[ ] Build thành công
[ ] App chạy được
[ ] Không có debug console output
[ ] Admin login OK
[ ] Giỏ hàng hoạt động tốt (nhanh, không lag)
```

---

## 🎉 Nếu tất cả PASS

**Chúc mừng!** Các thay đổi đã hoạt động tốt. Bạn đã sửa được:
- ✅ Security issue (hardcoded credentials)
- ✅ Performance issue (blocking async)
- ✅ Debug code leak
- ✅ Error logging

---

## ❌ Nếu có lỗi

Xem file `TEST_PLAN.md` để test chi tiết hơn, hoặc báo lỗi cho tôi!

---

## 🚀 PRODUCTION READY?

**Trước khi deploy:**

1. **ĐỔI MẬT KHẨU ADMIN** trong production:
   ```json
   // appsettings.Production.json
   {
     "AdminAccount": {
       "Password": "SuperSecurePassword123!@#"  // ← Đổi này!
     }
   }
   ```

2. **Update CORS** cho domain thật:
   ```json
   // appsettings.Production.json
   {
     "CorsSettings": {
       "AllowedOrigins": [
         "https://yourdomain.com"
       ]
     }
   }
   ```

3. **Test lại** trên production environment!

---

**Need help?** Xem `TEST_PLAN.md` để biết thêm chi tiết!
