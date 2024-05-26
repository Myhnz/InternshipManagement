document.addEventListener("DOMContentLoaded", function () {
  const searchIcon = document.querySelector(".search-icon");

  searchIcon.addEventListener("click", function () {
    this.classList.toggle("active");
    const searchInput = this.querySelector(".search-input");
    if (this.classList.contains("active")) {
      searchInput.focus();
    } else {
      searchInput.blur();
    }
  });
});

document.querySelectorAll(".form-outline").forEach((formOutline) => {
  new mdb.Input(formOutline).init();
});


function openChat() {
  document.getElementById("boxchat").style.display = "block";
}

function closeChat() {
  document.getElementById("boxchat").style.display = "none";
}
function toggleChat() {
  var boxchat = document.getElementById("boxchat");
  if (boxchat.style.display === "block") {
    boxchat.style.display = "none";
  } else {
    boxchat.style.display = "block";
  }
}

// Hàm để hiển thị thời gian của tin nhắn được nhấp vào
function showTimestamp(timestamp, messageElement) {
  var date = new Date(timestamp);
  var currentDate = new Date();

  var timeDiff = currentDate.getTime() - date.getTime();
  var secondsDiff = Math.floor(timeDiff / 1000);
  var minutesDiff = Math.floor(secondsDiff / 60);
  var hoursDiff = Math.floor(minutesDiff / 60);
  var daysDiff = Math.floor(hoursDiff / 24);

  var formattedTimestamp;

  if (daysDiff === 0) {
      // Tin nhắn gửi trong cùng một ngày, chỉ hiển thị giờ
      formattedTimestamp = date.toLocaleTimeString("vi-VN", {hour: "numeric", minute: "numeric", hour12: false});
  } else if (daysDiff < 7) {
      // Tin nhắn gửi trong tuần, hiển thị số ngày trước
      formattedTimestamp = daysDiff + " ngày trước";
  } else if (date.getFullYear() === currentDate.getFullYear()) {
      // Tin nhắn gửi trong năm, hiển thị ngày và tháng
      formattedTimestamp = date.toLocaleDateString("vi-VN", { day: "numeric", month: "short" });
  } else {
      // Tin nhắn gửi năm trước, hiển thị ngày, tháng và năm
      formattedTimestamp = date.toLocaleDateString("vi-VN", { day: "numeric", month: "short", year: "numeric" });
  }

  // Tạo phần tử mới để chứa thời gian
  var timestampElement = document.createElement("div");
  timestampElement.classList.add("timestamp", "text-center"); // Thêm lớp text-center
  var pElement = document.createElement("p"); // Tạo phần tử p
  pElement.textContent =  formattedTimestamp; // Thiết lập nội dung của phần tử p
  timestampElement.appendChild(pElement); // Thêm phần tử p vào phần tử timestamp

  // Chèn phần tử thời gian vào trước tin nhắn
  messageElement.parentNode.insertBefore(timestampElement, messageElement);

  // Xóa phần tử thời gian của tin nhắn trước đó (nếu có)
  var oldTimestampElement = document.querySelector(".timestamp.active");
  if (oldTimestampElement) {
      oldTimestampElement.parentNode.removeChild(oldTimestampElement);
  }

  // Thêm lớp 'active' cho phần tử thời gian mới để dễ dàng nhận biết và xử lý sau này
  timestampElement.classList.add("active");
}

// Thêm sự kiện click cho mỗi tin nhắn
document.querySelectorAll('.chat-message').forEach(item => {
  item.addEventListener('click', event => {
      // Hiển thị thời gian khi nhấp vào tin nhắn
      showTimestamp(item.dataset.timestamp, item);
  });
});


document.querySelector('.scrollable-div').addEventListener('scroll', function() {
  var div = this;
  var scrollToBottomBtn = document.getElementById('scrollToBottomBtn');

  if (div.scrollHeight - div.scrollTop === div.clientHeight || div.scrollHeight - div.scrollTop - 1 <= div.clientHeight+100) {
      // Đã cuộn xuống cuối
      scrollToBottomBtn.style.display = 'none';
  } else {
      // Chưa cuộn xuống cuối
      scrollToBottomBtn.style.display = 'block';
  }
});

document.getElementById('scrollToBottomBtn').addEventListener('click', function() {
  var div = document.querySelector('.scrollable-div');
  div.scrollTop = div.scrollHeight;
});

