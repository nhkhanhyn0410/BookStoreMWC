# Book Gallery Images Setup Guide

## Vấn đề: Thư viện ảnh chỉ hiển thị 1 ảnh bìa

Nếu trang chi tiết sách chỉ hiển thị ảnh bìa mà không có thư viện ảnh, có nghĩa là **database chưa có dữ liệu gallery images**.

---

## Giải pháp

### Option 1: Thêm dữ liệu mẫu bằng SQL (Nhanh nhất)

#### Bước 1: Chạy SQL Script
```sql
-- Cách 1: Dùng JSON field (Nhanh - Recommended)
UPDATE Books
SET AdditionalImages = '["https://picsum.photos/400/600?random=1","https://picsum.photos/400/600?random=2","https://picsum.photos/400/600?random=3","https://picsum.photos/400/600?random=4","https://picsum.photos/400/600?random=5"]'
WHERE Id = 1;  -- Thay đổi ID sách bạn muốn test

-- Cách 2: Tự động cho nhiều sách
UPDATE Books
SET AdditionalImages = CONCAT('[',
    '"https://picsum.photos/400/600?book=', Id, '-1",',
    '"https://picsum.photos/400/600?book=', Id, '-2",',
    '"https://picsum.photos/400/600?book=', Id, '-3",',
    '"https://picsum.photos/400/600?book=', Id, '-4"',
']')
WHERE IsActive = 1 AND Id <= 10;
```

#### Bước 2: Kiểm tra
```sql
SELECT Id, Title, AdditionalImages
FROM Books
WHERE Id = 1;
```

#### Bước 3: Refresh trang book details
- Mở `/books/{id}` trong browser
- Bạn sẽ thấy thư viện ảnh với 4-5 ảnh placeholder

---

### Option 2: Thêm vào bảng BookGalleryImages (Cấu trúc hơn)

```sql
-- Thêm gallery images cho Book ID 1
INSERT INTO BookGalleryImages
(BookId, ImageUrl, ImageFileName, ImageContentType, ImageFileSize, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
VALUES
(1, 'https://picsum.photos/400/600?book1-1', 'gallery1.jpg', 'image/jpeg', 50000, 1, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-2', 'gallery2.jpg', 'image/jpeg', 50000, 2, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-3', 'gallery3.jpg', 'image/jpeg', 50000, 3, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-4', 'gallery4.jpg', 'image/jpeg', 50000, 4, 1, GETUTCDATE(), GETUTCDATE());
```

**Lưu ý:** Code đã được cập nhật để tự động sync từ `BookGalleryImages` sang `AdditionalImages`.

---

### Option 3: Upload qua Admin Panel (Thực tế)

1. Đăng nhập vào Admin Panel
2. Vào Books > Edit Book
3. Upload gallery images (nếu UI hỗ trợ)
4. Save

---

## Cấu trúc dữ liệu

### AdditionalImages Field (JSON)
```json
[
  "https://picsum.photos/400/600?1",
  "https://picsum.photos/400/600?2",
  "https://picsum.photos/400/600?3"
]
```

### BookGalleryImages Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| BookId | int | Foreign key to Books |
| ImageUrl | nvarchar(500) | URL of the image |
| DisplayOrder | int | Order for display |
| IsActive | bit | Active flag |

---

## Debug: Kiểm tra dữ liệu

### 1. Kiểm tra AdditionalImages field
```sql
SELECT Id, Title, AdditionalImages,
       LEN(AdditionalImages) as JsonLength
FROM Books
WHERE Id = 1;
```

**Expected:**
- AdditionalImages: `["url1","url2",...]`
- JsonLength: > 0

### 2. Kiểm tra BookGalleryImages table
```sql
SELECT * FROM BookGalleryImages
WHERE BookId = 1 AND IsActive = 1
ORDER BY DisplayOrder;
```

**Expected:** Có ít nhất 1 record

### 3. Xem logs (nếu chạy ứng dụng)
```
Book 1 - AdditionalImages: ["https://..."], GalleryCount: 4, HasGallery: True
```

Nếu thấy `HasGallery: False`, database không có dữ liệu.

---

## Thay thế Placeholder Images bằng ảnh thật

### Bước 1: Upload ảnh lên server
- Upload vào `/wwwroot/images/books/gallery/`
- Đặt tên: `book1-gallery1.jpg`, `book1-gallery2.jpg`, etc.

### Bước 2: Cập nhật URLs
```sql
UPDATE Books
SET AdditionalImages = '[
  "/images/books/gallery/book1-gallery1.jpg",
  "/images/books/gallery/book1-gallery2.jpg",
  "/images/books/gallery/book1-gallery3.jpg"
]'
WHERE Id = 1;
```

---

## Script có sẵn

Chạy script đầy đủ tại: `Scripts/SeedGalleryImages.sql`

```bash
# SQL Server
sqlcmd -S localhost -d BookStoreDB -i Scripts/SeedGalleryImages.sql

# hoặc dùng SQL Server Management Studio
```

---

## Tính năng Gallery hiện tại

✅ Hiển thị tất cả ảnh trong thư viện
✅ Scrollable gallery với navigation buttons
✅ Click thumbnail để đổi ảnh chính
✅ Lightbox modal với thumbnail strip
✅ Keyboard navigation (Arrow keys, Escape)
✅ Touch swipe hỗ trợ mobile
✅ Smooth animations

---

## Troubleshooting

### Q: Vẫn chỉ thấy 1 ảnh sau khi update SQL?
A:
1. Clear browser cache (Ctrl+Shift+R)
2. Kiểm tra lại SQL query đã chạy thành công
3. Xem application logs có lỗi không

### Q: Muốn thêm nhiều ảnh hơn?
A: Chỉnh JSON array trong AdditionalImages, có thể thêm tới 20-30 ảnh.

### Q: Ảnh không load?
A: Kiểm tra URL ảnh có accessible không. Thử mở trực tiếp trong browser.

### Q: Làm sao để upload ảnh thật qua UI?
A: Cần implement upload feature trong Admin Panel. Hiện tại dùng SQL để seed data.

---

## Next Steps

Sau khi có gallery images, bạn có thể:
1. Test các tính năng: navigation, lightbox, swipe
2. Thêm ảnh thật cho các sách khác
3. Implement upload UI trong admin panel
4. Tối ưu hóa image loading (lazy load, CDN)
