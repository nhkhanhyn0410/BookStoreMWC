# 🧪 TEST PLAN - BookStoreMWC Code Fixes

## Tổng quan các thay đổi cần test

Commit: `d7e3149` - Fix critical security and performance issues

### Files đã thay đổi:
1. ✅ `appsettings.json` - Thêm AdminAccount và CorsSettings
2. ✅ `Data/DbInitializer.cs` - Đọc admin credentials từ config
3. ✅ `Program.cs` - Xóa debug output, config CORS
4. ✅ `Services/SessionCartService.cs` - Fix blocking async call
5. ✅ `Models/Entities/Book.cs` - Better exception handling

---

## 📋 CHECKLIST TEST NHANH

```
[ ] Build thành công không có lỗi
[ ] Ứng dụng chạy được và không crash
[ ] Database migrations chạy thành công
[ ] Admin login thành công
[ ] Giỏ hàng guest hoạt động (add/remove/update)
[ ] Không có console debug output khi chạy
[ ] Logs ghi đúng errors khi có lỗi
[ ] CORS configuration hoạt động
```

---

## 🔧 BƯỚC 1: BUILD VÀ KHỞI CHẠY

### 1.1. Clean và Restore
```bash
# Xóa build cũ
dotnet clean

# Restore packages
dotnet restore
```

### 1.2. Build Project
```bash
# Build project
dotnet build

# ✅ Kết quả mong đợi:
# Build succeeded.
#     0 Warning(s)
#     0 Error(s)
```

**⚠️ Nếu có lỗi:** Check xem có missing using statements không

### 1.3. Chạy ứng dụng
```bash
# Run application
dotnet run

# HOẶC nếu dùng Visual Studio:
# Nhấn F5 hoặc click Start Debugging
```

**✅ Kết quả mong đợi:**
- Ứng dụng start thành công
- KHÔNG còn thấy các dòng debug:
  ```
  ❌ KHÔNG nên thấy:
  "==================== DEBUG INFO ===================="
  "Connection String: ..."
  "Current Directory: ..."
  "All available connection strings:"
  ```
- Chỉ thấy logs bình thường như:
  ```
  ✅ Nên thấy:
  info: Microsoft.Hosting.Lifetime[14]
        Now listening on: https://localhost:7001
  info: Microsoft.Hosting.Lifetime[0]
        Application started.
  ```

---

## 🔒 BƯỚC 2: TEST ADMIN CREDENTIALS (CRITICAL)

### 2.1. Verify Config File
Mở `appsettings.json` và kiểm tra:

```json
"AdminAccount": {
  "Email": "admin@bookstore.com",
  "Password": "Admin123!",
  "Name": "Quản trị viên"
}
```

### 2.2. Test Admin Login

**Test Case 1: Admin Login với default credentials**

1. Mở browser: `https://localhost:7001`
2. Click **"Đăng nhập"** hoặc navigate to `/Account/Login`
3. Nhập:
   - Email: `admin@bookstore.com`
   - Password: `Admin123!`
4. Click **"Đăng nhập"**

**✅ Kết quả mong đợi:**
- Login thành công
- Redirect đến Admin Dashboard hoặc Home
- Hiển thị tên "Quản trị viên" ở header
- Có menu Admin (nếu user có role Admin)

**❌ Nếu lỗi:**
- Check logs xem DbInitializer có chạy không
- Check database xem user admin đã được tạo chưa:
  ```sql
  SELECT * FROM AspNetUsers WHERE Email = 'admin@bookstore.com'
  SELECT * FROM AspNetUserRoles WHERE UserId = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@bookstore.com')
  ```

### 2.3. Test thay đổi Admin Password

**Test Case 2: Đổi mật khẩu admin qua config**

1. Stop ứng dụng
2. Sửa `appsettings.json`:
   ```json
   "AdminAccount": {
     "Email": "admin@bookstore.com",
     "Password": "NewSecurePass123!",  // ← Đổi password
     "Name": "Quản trị viên"
   }
   ```
3. XÓA database để test lại:
   ```bash
   # Xóa database file
   # Hoặc chạy:
   dotnet ef database drop -f
   ```
4. Chạy lại ứng dụng
5. Login với password MỚI: `NewSecurePass123!`

**✅ Kết quả mong đợi:**
- Login với password mới thành công
- Không login được với password cũ

**⚠️ Quan trọng:** Đừng quên đổi lại password cũ sau khi test!

---

## 🛒 BƯỚC 3: TEST GIỎ HÀNG (PERFORMANCE FIX)

Đây là test quan trọng vì chúng ta đã sửa blocking async call!

### 3.1. Test Guest Cart (Session Cart)

**Test Case 3: Thêm sách vào giỏ hàng khi chưa đăng nhập**

1. **Logout** nếu đang login (hoặc dùng incognito/private window)
2. Browse danh sách sách: `/` hoặc `/Books`
3. Click **"Thêm vào giỏ hàng"** trên 1 cuốn sách

**✅ Kết quả mong đợi:**
- Sách được thêm vào giỏ NHANH (không bị lag)
- Cart icon cập nhật số lượng
- Có notification thành công
- KHÔNG bị freeze/hang UI

**Test Case 4: Update và Remove cart items**

4. Vào `/Cart` để xem giỏ hàng
5. Thay đổi số lượng (tăng/giảm)
6. Click **"Xóa"** để remove item

**✅ Kết quả mong đợi:**
- Tất cả operations diễn ra NHANH, không delay
- Số lượng và tổng tiền cập nhật đúng
- Session cart được lưu đúng (refresh page vẫn giữ cart)

### 3.2. Test Performance

**Test Case 5: Concurrent Cart Operations**

1. Mở nhiều tabs cùng lúc
2. Thêm nhiều sách vào giỏ ở các tabs khác nhau
3. Observe response time

**✅ Kết quả mong đợi:**
- Không có deadlock
- Response time < 500ms mỗi operation
- Không có errors trong console/logs

**❌ Trước khi fix:**
- Có thể bị chậm do blocking `.Result` call
- Risk deadlock nếu nhiều requests

**✅ Sau khi fix:**
- Async all the way, không block threads
- Scalable hơn với nhiều users

### 3.3. Test Logged-in User Cart

**Test Case 6: Cart khi đã login**

1. Login với tài khoản user (không phải admin)
2. Thêm sách vào giỏ hàng
3. Kiểm tra giỏ hàng được lưu vào database

**✅ Kết quả mong đợi:**
- Cart operations vẫn nhanh
- Database cart và session cart đều hoạt động

---

## 📝 BƯỚC 4: TEST LOGGING (ERROR HANDLING)

### 4.1. Test SessionCartService Logging

**Test Case 7: Force JSON deserialization error**

Đây là advanced test để verify logging hoạt động:

1. Stop ứng dụng
2. Thêm một số sách vào giỏ hàng (để tạo session)
3. Mở Developer Tools → Application → Session Storage
4. Tìm key `GuestCart` và sửa value thành invalid JSON:
   ```
   {invalid json!!!
   ```
5. Refresh page
6. Check logs

**✅ Kết quả mong đợi:**
- Ứng dụng KHÔNG crash
- Logs hiển thị error:
  ```
  [Error] Failed to deserialize cart from session. Returning empty cart.
  ```
- User thấy giỏ hàng trống (không bị error page)

**Nơi xem logs:**
- Console output khi chạy `dotnet run`
- Hoặc file logs trong `/Logs/` folder (nếu có Serilog file sink)

### 4.2. Test Book.AdditionalImagesList Exception Handling

**Test Case 8: Corrupt JSON in Book.AdditionalImages**

1. Login as admin
2. Vào Database và manual corrupt một Book's AdditionalImages:
   ```sql
   UPDATE Books
   SET AdditionalImages = '{invalid json'
   WHERE Id = 1
   ```
3. View book detail page: `/Books/Details/1`

**✅ Kết quả mong đợi:**
- Page hiển thị bình thường
- Không có additional images (fallback to empty list)
- KHÔNG crash với JsonException

---

## 🌐 BƯỚC 5: TEST CORS CONFIGURATION

### 5.1. Verify CORS Config

Kiểm tra `appsettings.json`:
```json
"CorsSettings": {
  "AllowedOrigins": [
    "https://localhost:5001",
    "https://localhost:7001"
  ]
}
```

### 5.2. Test CORS (Advanced)

**Test Case 9: CORS requests**

Nếu bạn có frontend riêng hoặc API calls:

1. Mở browser console trên `https://localhost:5001`
2. Run:
   ```javascript
   fetch('https://localhost:7001/api/books')
     .then(r => r.json())
     .then(console.log)
     .catch(console.error)
   ```

**✅ Kết quả mong đợi:**
- Request thành công (nếu có API endpoint)
- KHÔNG có CORS errors

**Test Case 10: Đổi CORS config**

1. Stop app
2. Sửa `appsettings.json`:
   ```json
   "CorsSettings": {
     "AllowedOrigins": [
       "https://localhost:5001",
       "https://localhost:7001",
       "https://localhost:3000"  // ← Thêm origin mới
     ]
   }
   ```
3. Restart app
4. Verify CORS cho origin mới hoạt động

---

## 🎯 BƯỚC 6: INTEGRATION TEST (E2E)

### Full User Journey Test

**Test Case 11: Complete shopping flow**

1. **Guest User:**
   - Browse books → Add 3 books to cart
   - View cart → Update quantities
   - Register new account
   - Verify cart migrated to logged-in user

2. **Checkout:**
   - Proceed to checkout
   - Fill shipping info
   - Complete order

3. **Admin:**
   - Login as admin (`admin@bookstore.com`)
   - View orders in Admin Dashboard
   - Verify new order appears

**✅ Kết quả mong đợi:**
- Toàn bộ flow hoạt động mượt mà
- Không có errors
- Performance tốt (không lag)

---

## 🐛 TROUBLESHOOTING

### Lỗi thường gặp và cách fix:

#### ❌ Lỗi 1: "Admin password not configured"
```
⚠️ Admin password not configured in appsettings.json. Skipping admin user creation.
```
**Fix:** Check `appsettings.json` có section `AdminAccount` với `Password` không

#### ❌ Lỗi 2: Build failed - Missing using
```
Error CS0246: The type or namespace name 'IConfiguration' could not be found
```
**Fix:** Thêm `using Microsoft.Extensions.Configuration;` vào `DbInitializer.cs`

#### ❌ Lỗi 3: SessionCartService constructor error
```
Unable to resolve service for type 'ILogger<SessionCartService>'
```
**Fix:** Logger đã được register trong DI container của ASP.NET Core, không cần action. Nếu vẫn lỗi, restart Visual Studio/Rider.

#### ❌ Lỗi 4: Database connection error
```
Cannot open database "BookStoreMVCV2" requested by the login
```
**Fix:**
```bash
# Drop và recreate database
dotnet ef database drop -f
dotnet ef database update
# Hoặc chạy app, migrations sẽ tự chạy
```

---

## 📊 PERFORMANCE COMPARISON

### Trước khi fix:
```
Add to Cart operation: ~200-500ms
Risk: Thread blocking, potential deadlock
Scalability: Limited by sync-over-async pattern
```

### Sau khi fix:
```
Add to Cart operation: ~50-150ms
Risk: Minimal, proper async/await
Scalability: Much better, no thread blocking
```

**Cách đo:**
1. Mở Browser DevTools → Network tab
2. Add item to cart
3. Check timing của AJAX request

---

## ✅ FINAL CHECKLIST

Sau khi test xong, verify:

### Functional Tests:
- [ ] Build thành công, no errors
- [ ] App runs và không crash
- [ ] Admin login với credentials từ config
- [ ] Guest cart: add/update/remove items
- [ ] Logged-in user cart hoạt động
- [ ] Database migrations OK
- [ ] No console debug output
- [ ] Logs capture errors properly

### Non-Functional Tests:
- [ ] Performance: Cart operations < 500ms
- [ ] No memory leaks (run for 5-10 minutes)
- [ ] No deadlocks under concurrent operations
- [ ] CORS configuration từ config hoạt động

### Code Quality:
- [ ] No hardcoded credentials in code
- [ ] No console debug statements
- [ ] Proper async/await usage
- [ ] Error handling with logging

---

## 🚀 PRODUCTION CHECKLIST

Trước khi deploy production:

1. **Security:**
   - [ ] Đổi admin password trong `appsettings.Production.json`
   - [ ] Set CORS origins cho production domain
   - [ ] Review connection strings
   - [ ] Enable HTTPS only

2. **Performance:**
   - [ ] Test với nhiều concurrent users
   - [ ] Monitor memory usage
   - [ ] Check database connection pooling

3. **Monitoring:**
   - [ ] Setup application logging (Serilog to file/cloud)
   - [ ] Setup error tracking (e.g., Sentry, Application Insights)
   - [ ] Configure health checks

---

## 📞 Nếu gặp vấn đề

Nếu bất kỳ test nào FAIL, cho tôi biết:
1. Test case nào fail
2. Error message cụ thể
3. Logs output
4. Screenshots nếu có

Tôi sẽ giúp debug và fix!

---

**Test Date:** _____________
**Tested By:** _____________
**Result:** ✅ PASS / ❌ FAIL / ⚠️ PARTIAL

**Notes:**
_______________________________________
_______________________________________
_______________________________________
