/**
 * Admin Panel JavaScript - Complete Version
 * Main functionality for BookStore Admin Panel
 */

class AdminPanel {
    constructor() {
        this.searchTimeout = null;
        this.notificationInterval = null;
        this.init();
    }

    init() {
        this.initSearch();
        this.initNotifications();
        this.initDataTables();
        this.initFormValidation();
        this.initFileUploads();
        this.initCharts();
        this.initTooltips();
    }

    /**
     * Admin Search with Autocomplete
     */
    initSearch() {
        const searchInput = $('#admin-search');
        const suggestionsBox = $('#search-suggestions');
        
        if (!searchInput.length) return;

        searchInput.on('input', (e) => {
            const query = e.target.value.trim();
            
            // Clear previous timeout
            if (this.searchTimeout) {
                clearTimeout(this.searchTimeout);
            }

            if (query.length < 2) {
                suggestionsBox.addClass('hidden').empty();
                return;
            }

            // Debounce search
            this.searchTimeout = setTimeout(() => {
                this.performSearch(query);
            }, 300);
        });

        // Close suggestions when clicking outside
        $(document).on('click', (e) => {
            if (!$(e.target).closest('#admin-search, #search-suggestions').length) {
                suggestionsBox.addClass('hidden');
            }
        });

        // Handle keyboard navigation
        searchInput.on('keydown', (e) => {
            if (e.key === 'Escape') {
                suggestionsBox.addClass('hidden');
            }
        });
    }

    performSearch(query) {
        const suggestionsBox = $('#search-suggestions');
        
        // Show loading state
        suggestionsBox.removeClass('hidden').html(`
            <div class="p-4 text-center">
                <i class="fas fa-spinner fa-spin text-gray-400 mr-2"></i>
                <span class="text-sm text-gray-600">Đang tìm kiếm...</span>
            </div>
        `);

        // Simulate search API call
        $.ajax({
            url: '/Admin/Search',
            method: 'GET',
            data: { q: query },
            success: (response) => {
                this.displaySearchResults(response);
            },
            error: () => {
                suggestionsBox.html(`
                    <div class="p-4 text-center text-red-600">
                        <i class="fas fa-exclamation-circle mr-2"></i>
                        <span class="text-sm">Lỗi khi tìm kiếm</span>
                    </div>
                `);
            }
        });
    }

    displaySearchResults(results) {
        const suggestionsBox = $('#search-suggestions');
        
        if (!results || (!results.books?.length && !results.orders?.length && !results.users?.length)) {
            suggestionsBox.html(`
                <div class="p-4 text-center text-gray-500">
                    <i class="fas fa-search text-2xl mb-2"></i>
                    <p class="text-sm">Không tìm thấy kết quả</p>
                </div>
            `);
            return;
        }

        let html = '<div class="py-2">';
        
        // Books
        if (results.books?.length) {
            html += '<div class="px-4 py-2 text-xs font-semibold text-gray-500 uppercase">Sách</div>';
            results.books.forEach(book => {
                html += `
                    <a href="/Admin/Books/Edit/${book.id}" class="block px-4 py-2 hover:bg-gray-50 transition-colors">
                        <div class="flex items-center">
                            <i class="fas fa-book text-blue-500 mr-3"></i>
                            <div class="flex-1">
                                <p class="text-sm font-medium text-gray-900">${book.title}</p>
                                <p class="text-xs text-gray-500">${book.author}</p>
                            </div>
                        </div>
                    </a>
                `;
            });
        }
        
        // Orders
        if (results.orders?.length) {
            html += '<div class="px-4 py-2 text-xs font-semibold text-gray-500 uppercase mt-2">Đơn hàng</div>';
            results.orders.forEach(order => {
                html += `
                    <a href="/Admin/Orders/Details/${order.id}" class="block px-4 py-2 hover:bg-gray-50 transition-colors">
                        <div class="flex items-center">
                            <i class="fas fa-shopping-cart text-green-500 mr-3"></i>
                            <div class="flex-1">
                                <p class="text-sm font-medium text-gray-900">Đơn hàng #${order.id}</p>
                                <p class="text-xs text-gray-500">${order.customerName} - ${order.totalAmount}</p>
                            </div>
                        </div>
                    </a>
                `;
            });
        }
        
        // Users
        if (results.users?.length) {
            html += '<div class="px-4 py-2 text-xs font-semibold text-gray-500 uppercase mt-2">Người dùng</div>';
            results.users.forEach(user => {
                html += `
                    <a href="/Admin/Users/Details/${user.id}" class="block px-4 py-2 hover:bg-gray-50 transition-colors">
                        <div class="flex items-center">
                            <i class="fas fa-user text-purple-500 mr-3"></i>
                            <div class="flex-1">
                                <p class="text-sm font-medium text-gray-900">${user.name}</p>
                                <p class="text-xs text-gray-500">${user.email}</p>
                            </div>
                        </div>
                    </a>
                `;
            });
        }
        
        html += '</div>';
        suggestionsBox.html(html);
    }

    /**
     * Notifications System
     */
    initNotifications() {
        // Load initial notifications
        this.loadNotifications();
        
        // Poll for new notifications every 30 seconds
        this.notificationInterval = setInterval(() => {
            this.loadNotifications();
        }, 30000);
        
        // Cleanup on page unload
        $(window).on('beforeunload', () => {
            if (this.notificationInterval) {
                clearInterval(this.notificationInterval);
            }
        });
    }

    loadNotifications() {
        $.ajax({
            url: '/Admin/Notifications/GetUnread',
            method: 'GET',
            success: (response) => {
                this.updateNotificationBadge(response.count);
                if (response.notifications?.length) {
                    this.displayNotifications(response.notifications);
                }
            },
            error: (xhr) => {
                console.error('Failed to load notifications:', xhr);
            }
        });
    }

    updateNotificationBadge(count) {
        const badge = $('#notification-badge');
        if (count > 0) {
            badge.text(count).removeClass('hidden');
        } else {
            badge.addClass('hidden');
        }
    }

    displayNotifications(notifications) {
        const list = $('#notifications-list');
        if (!list.length) return;

        if (!notifications || notifications.length === 0) {
            list.html(`
                <div class="px-4 py-8 text-center text-gray-500">
                    <i class="fas fa-bell-slash text-3xl mb-2"></i>
                    <p class="text-sm">Không có thông báo mới</p>
                </div>
            `);
            return;
        }

        let html = '';
        notifications.forEach(notif => {
            const icon = this.getNotificationIcon(notif.type);
            const time = this.formatTime(notif.createdAt);
            
            html += `
                <a href="${notif.url || '#'}" class="block px-4 py-3 hover:bg-gray-50 border-b border-gray-100 transition-colors ${!notif.isRead ? 'bg-blue-50' : ''}">
                    <div class="flex items-start">
                        <div class="flex-shrink-0">
                            <i class="${icon} text-lg"></i>
                        </div>
                        <div class="ml-3 flex-1">
                            <p class="text-sm font-medium text-gray-900">${notif.title}</p>
                            <p class="text-xs text-gray-600 mt-1">${notif.message}</p>
                            <p class="text-xs text-gray-400 mt-1">${time}</p>
                        </div>
                    </div>
                </a>
            `;
        });

        list.html(html);
    }

    getNotificationIcon(type) {
        const icons = {
            'order': 'fas fa-shopping-cart text-green-500',
            'user': 'fas fa-user text-blue-500',
            'review': 'fas fa-star text-yellow-500',
            'system': 'fas fa-cog text-gray-500',
            'warning': 'fas fa-exclamation-triangle text-orange-500',
            'error': 'fas fa-exclamation-circle text-red-500'
        };
        return icons[type] || 'fas fa-bell text-gray-500';
    }

    formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        
        if (diffMins < 1) return 'Vừa xong';
        if (diffMins < 60) return `${diffMins} phút trước`;
        
        const diffHours = Math.floor(diffMins / 60);
        if (diffHours < 24) return `${diffHours} giờ trước`;
        
        const diffDays = Math.floor(diffHours / 24);
        if (diffDays < 7) return `${diffDays} ngày trước`;
        
        return date.toLocaleDateString('vi-VN');
    }

    /**
     * DataTables Enhancement
     */
    initDataTables() {
        if (typeof $.fn.DataTable === 'undefined') return;

        $('.admin-datatable').each(function() {
            if (!$(this).hasClass('dataTable')) {
                $(this).DataTable({
                    language: {
                        url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/vi.json'
                    },
                    responsive: true,
                    pageLength: 25,
                    order: [[0, 'desc']]
                });
            }
        });
    }

    /**
     * Form Validation
     */
    initFormValidation() {
        $('form[data-ajax="true"]').on('submit', function(e) {
            e.preventDefault();
            
            const form = $(this);
            const submitBtn = form.find('button[type="submit"]');
            
            // Add loading state
            submitBtn.addClass('btn-loading').prop('disabled', true);
            
            $.ajax({
                url: form.attr('action'),
                method: form.attr('method') || 'POST',
                data: form.serialize(),
                success: (response) => {
                    if (response.success) {
                        showNotification(response.message || 'Thành công!', 'success');
                        if (response.redirectUrl) {
                            setTimeout(() => {
                                window.location.href = response.redirectUrl;
                            }, 1000);
                        }
                    } else {
                        showNotification(response.message || 'Có lỗi xảy ra!', 'error');
                    }
                },
                error: (xhr) => {
                    showNotification('Có lỗi xảy ra. Vui lòng thử lại!', 'error');
                    console.error('Form submission error:', xhr);
                },
                complete: () => {
                    submitBtn.removeClass('btn-loading').prop('disabled', false);
                }
            });
        });
    }

    /**
     * File Upload with Preview
     */
    initFileUploads() {
        $('input[type="file"][data-preview]').on('change', function(e) {
            const file = e.target.files[0];
            const previewId = $(this).data('preview');
            const preview = $(`#${previewId}`);
            
            if (file && preview.length) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    preview.attr('src', e.target.result).removeClass('hidden');
                };
                reader.readAsDataURL(file);
            }
        });
    }

    /**
     * Charts Initialization
     */
    initCharts() {
        // Revenue Chart
        const revenueCanvas = document.getElementById('revenueChart');
        if (revenueCanvas && typeof Chart !== 'undefined') {
            const ctx = revenueCanvas.getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12'],
                    datasets: [{
                        label: 'Doanh thu',
                        data: window.chartData?.revenue || [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                        borderColor: 'rgb(59, 130, 246)',
                        backgroundColor: 'rgba(59, 130, 246, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    }
                }
            });
        }
    }

    /**
     * Tooltips
     */
    initTooltips() {
        $('[data-tooltip]').each(function() {
            const text = $(this).data('tooltip');
            $(this).attr('title', text);
        });
    }
}

/**
 * Utility Functions
 */

// Show notification toast
function showNotification(message, type = 'info', duration = 3000) {
    const types = {
        success: { icon: 'fa-check-circle', color: 'green' },
        error: { icon: 'fa-times-circle', color: 'red' },
        warning: { icon: 'fa-exclamation-triangle', color: 'yellow' },
        info: { icon: 'fa-info-circle', color: 'blue' }
    };
    
    const config = types[type] || types.info;
    
    const notification = $(`
        <div class="notification fixed top-4 right-4 z-50 max-w-sm w-full bg-white border border-${config.color}-200 rounded-lg shadow-lg transform transition-all duration-300 translate-x-0">
            <div class="p-4">
                <div class="flex items-start">
                    <div class="flex-shrink-0">
                        <i class="fas ${config.icon} text-${config.color}-500 text-xl"></i>
                    </div>
                    <div class="ml-3 flex-1">
                        <p class="text-sm font-medium text-gray-900">${message}</p>
                    </div>
                    <button class="ml-4 flex-shrink-0 text-gray-400 hover:text-gray-600" onclick="$(this).closest('.notification').remove()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>
        </div>
    `);
    
    $('body').append(notification);
    
    setTimeout(() => {
        notification.fadeOut(300, function() {
            $(this).remove();
        });
    }, duration);
}

// Confirm action
function confirmAction(message, callback) {
    if (confirm(message)) {
        if (typeof callback === 'function') {
            callback();
        }
        return true;
    }
    return false;
}

// Show loading spinner
function showLoading(text = 'Đang tải...') {
    const spinner = $('#loadingSpinner');
    if (spinner.length) {
        spinner.find('#loadingText').text(text);
        spinner.removeClass('hidden');
    }
}

// Hide loading spinner
function hideLoading() {
    $('#loadingSpinner').addClass('hidden');
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Format number
function formatNumber(num) {
    return new Intl.NumberFormat('vi-VN').format(num);
}

// Initialize when DOM is ready
$(document).ready(function() {
    // Create global admin panel instance
    if (typeof AdminPanel !== 'undefined') {
        window.adminPanel = new AdminPanel();
    }
    
    // Handle notification dropdown toggle
    $('#notifications-btn').on('click', function(e) {
        e.stopPropagation();
        $('#notifications-dropdown').toggleClass('hidden');
    });
    
    // Close dropdown when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('#notifications-btn, #notifications-dropdown').length) {
            $('#notifications-dropdown').addClass('hidden');
        }
    });
});