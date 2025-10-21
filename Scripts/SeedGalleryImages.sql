-- Script to add sample gallery images to books
-- This adds multiple gallery images to existing books for testing

-- Method 1: Using AdditionalImages JSON field (Recommended for quick testing)
-- Update books with JSON array of image URLs

UPDATE Books
SET AdditionalImages = '["https://picsum.photos/400/600?random=1","https://picsum.photos/400/600?random=2","https://picsum.photos/400/600?random=3","https://picsum.photos/400/600?random=4","https://picsum.photos/400/600?random=5"]'
WHERE Id IN (1, 2, 3, 4, 5);

-- Or use placeholder images with book-specific numbers
UPDATE Books
SET AdditionalImages = CONCAT('[',
    '"https://picsum.photos/400/600?book=', Id, '-1",',
    '"https://picsum.photos/400/600?book=', Id, '-2",',
    '"https://picsum.photos/400/600?book=', Id, '-3",',
    '"https://picsum.photos/400/600?book=', Id, '-4"',
']')
WHERE Id <= 10 AND IsActive = 1;

-- Method 2: Using BookGalleryImages table (More structured approach)
-- Insert records into BookGalleryImages table

-- For Book ID 1
INSERT INTO BookGalleryImages (BookId, ImageUrl, ImageFileName, ImageContentType, ImageFileSize, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
VALUES
(1, 'https://picsum.photos/400/600?book1-1', 'gallery1-1.jpg', 'image/jpeg', 50000, 1, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-2', 'gallery1-2.jpg', 'image/jpeg', 50000, 2, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-3', 'gallery1-3.jpg', 'image/jpeg', 50000, 3, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-4', 'gallery1-4.jpg', 'image/jpeg', 50000, 4, 1, GETUTCDATE(), GETUTCDATE()),
(1, 'https://picsum.photos/400/600?book1-5', 'gallery1-5.jpg', 'image/jpeg', 50000, 5, 1, GETUTCDATE(), GETUTCDATE());

-- For Book ID 2
INSERT INTO BookGalleryImages (BookId, ImageUrl, ImageFileName, ImageContentType, ImageFileSize, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
VALUES
(2, 'https://picsum.photos/400/600?book2-1', 'gallery2-1.jpg', 'image/jpeg', 50000, 1, 1, GETUTCDATE(), GETUTCDATE()),
(2, 'https://picsum.photos/400/600?book2-2', 'gallery2-2.jpg', 'image/jpeg', 50000, 2, 1, GETUTCDATE(), GETUTCDATE()),
(2, 'https://picsum.photos/400/600?book2-3', 'gallery2-3.jpg', 'image/jpeg', 50000, 3, 1, GETUTCDATE(), GETUTCDATE());

-- For Book ID 3
INSERT INTO BookGalleryImages (BookId, ImageUrl, ImageFileName, ImageContentType, ImageFileSize, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
VALUES
(3, 'https://picsum.photos/400/600?book3-1', 'gallery3-1.jpg', 'image/jpeg', 50000, 1, 1, GETUTCDATE(), GETUTCDATE()),
(3, 'https://picsum.photos/400/600?book3-2', 'gallery3-2.jpg', 'image/jpeg', 50000, 2, 1, GETUTCDATE(), GETUTCDATE()),
(3, 'https://picsum.photos/400/600?book3-3', 'gallery3-3.jpg', 'image/jpeg', 50000, 3, 1, GETUTCDATE(), GETUTCDATE()),
(3, 'https://picsum.photos/400/600?book3-4', 'gallery3-4.jpg', 'image/jpeg', 50000, 4, 1, GETUTCDATE(), GETUTCDATE());

-- Verify the data
SELECT
    b.Id,
    b.Title,
    b.AdditionalImages,
    COUNT(g.Id) as GalleryImageCount
FROM Books b
LEFT JOIN BookGalleryImages g ON b.Id = g.BookId AND g.IsActive = 1
WHERE b.IsActive = 1
GROUP BY b.Id, b.Title, b.AdditionalImages
ORDER BY b.Id;

-- Check specific book's gallery images
SELECT * FROM BookGalleryImages WHERE BookId = 1 ORDER BY DisplayOrder;
