
function toggleMenu() {
    var settingMenu = document.getElementById("setting");
    var notificationDropdown = document.getElementById("noti"); 
    notificationDropdown.style.display = "none";

    if (settingMenu.style.display === "none") {
        settingMenu.style.display = "block";
    } else {
        settingMenu.style.display = "none";
    }
}


function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    const content = document.getElementById("content");
    sidebar.classList.toggle("collapsed");
    content.classList.toggle("collapsed");
}
// Hàm kiểm tra kích thước màn hình và cập nhật class của sidebar và content
function checkScreenWidth() {
    const sidebar = document.getElementById("sidebar");
    const content = document.getElementById("content");
    const screenWidth = window.innerWidth;

    if (screenWidth <= 991) {
        sidebar.classList.add("collapsed");
        content.classList.add("collapsed");
    } else {
        sidebar.classList.remove("collapsed");
        content.classList.remove("collapsed");
    }
}

// Gọi hàm kiểm tra khi trang web được tải và khi kích thước màn hình thay đổi
document.addEventListener("DOMContentLoaded", checkScreenWidth);
window.addEventListener("resize", checkScreenWidth);


$(document).ready(function () {
    // Xử lý sự kiện click cho các phần tử trong '.sidebar1 a'
    $('.sidebar1 a.multitabs').click(function () {
        $('.sidebar1 a').removeClass('active');
        $(this).addClass('active');
    });

    // Xử lý sự kiện click cho các phần tử trong 'nav.mt-nav ul li a'
    $('nav.mt-nav ul li a').click(function () {
        var href = $(this).attr('data-url');
        $('.sidebar1 a').removeClass('active');
        $('.sidebar1 a.multitabs[href="' + href + '"]').addClass('active');
    });

    // Thêm sự kiện 'click' cho các phần tử được tạo mới trong 'nav.mt-nav ul li a'
    $(document).on('click', 'nav.mt-nav ul li a', function () {
        var href = $(this).attr('data-url');
        $('.sidebar1 a').removeClass('active');
        $('.sidebar1 a.multitabs[href="' + href + '"]').addClass('active');
        // Kiểm tra nếu '.sidebar1 .collapse a.active' tồn tại và '.sidebar1 .collapse' chưa có class 'show', thêm class 'show' vào '.sidebar1 .collapse'
        if ($('.sidebar1 .collapse a.active').length > 0 && !$('.sidebar1 .collapse').hasClass('show')) {
            $('.sidebar1 .collapse').addClass('show'); $('.fas.fa-angle-left').addClass('rotate-90');
        }
    });
});



//icon < xoay
$(document).ready(function () {
    $('#collapseExample').on('show.bs.collapse', function () {
        $('.sidebar-down').addClass('rotate-90');
    });

    $('#collapseExample').on('hide.bs.collapse', function () {
        $('.sidebar-down').removeClass('rotate-90');
    });
});

