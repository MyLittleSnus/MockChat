const connection = new signalR.HubConnectionBuilder()
	.withUrl("/hubs/chats")
	.build();

const baseUrl = "http://localhost:5167";

let currentUser;

document.getElementById("messageText").addEventListener("keydown", function (event) {
	if (event.key === "Enter") {
		sendMessage(window.ChatName);
		event.preventDefault();
	}
});
connection.start().then(() => {
	connection.invoke("Sync", getCookie("userId"));
}).catch(err => console.error(err.toString()));

connection.on("onSync", (user) => {
	console.log("Synced user:", user);
	currentUser = user;
	setCookie("userId", user.id, 365);
});

connection.on("onSessionInvalid", sessionInvalid => {
	if (sessionInvalid) {
		console.error("User is not authorized, redirecting to home page...");
		window.location.href = baseUrl;
	}
});

connection.on("onMessageReceived", (message) => {
	displayMessage(message);
});

connection.on("onInvited", (chat) => {
	console.log(`received invite from chat ${chat.name}`);
	displayInvitedToast(chat);
});

connection.on("onInviteResult", (result, user, error) => {
	console.log("invite result: " + result);
	displayInviteResultToast(user, result, error);
});

function sendMessage (chatName) {
	let messageBox =  document.getElementById("messageText");
	const messageText = messageBox.value;
	messageBox.value = '';
	connection.invoke("SendMessageInChat", chatName, messageText);
	scrollToBottom();
}

function displayMessage (message) {
	const chatMessages = document.getElementById("chatMessages");
	const messageBubble = document.createElement("div");
	messageBubble.classList.add("message-bubble");
	const formattedTime = formatMessageTime(new Date());
	messageBubble.innerHTML = `<strong>${message.sender.username}:</strong><br>${message.content}<br><span class="message-time">${formattedTime}</span>`;
	
	const isCurrentUser = message.sender.id.toString() === getCookie("userId");
	
	if (isCurrentUser) {
		messageBubble.classList.add("current-user");
	} else {
		messageBubble.classList.add("other-user");
	}
	
	chatMessages.appendChild(messageBubble);
}

function scrollToBottom() {
	const chatMessages = document.getElementById("chatMessages");
	chatMessages.scrollTop = chatMessages.scrollHeight + document.getElementById("messageInput").height;
}

function displayInvitedToast (chat) {
	const toast = document.getElementById("onInvitedToast");
	const toastText = document.getElementById("onInvitedToastText");
	toastText.innerHTML = `You have been invited to chat ${chat.name}`;
	const invokeToast = bootstrap.Toast.getOrCreateInstance(toast);
	invokeToast.show();
}

function displayInviteResultToast(user, inviteResult, error){
	const toast = document.getElementById("inviteResultToast");
	const toastText = document.getElementById("inviteResultToastText");
	if (inviteResult){
		toastText.innerHTML = `You have invited ${user.username}`;
	} else {
		toastText.innerHTML = error;
	}
	const invokeToast = bootstrap.Toast.getOrCreateInstance(toast);
	invokeToast.show();
}

function inviteUser (chatName) {
	const username = prompt("Enter username:");
	if (username) {
		connection.invoke("InviteToChat", username, chatName);
	}
}

function formatMessageTime (date) {
	const hours = date.getHours().toString().padStart(2, '0');
	const minutes = date.getMinutes().toString().padStart(2, '0');
	return `${hours}:${minutes}`;
}

function setCookie (name, value, days) {
	const expires = new Date(Date.now() + days * 24 * 60 * 60 * 1000).toUTCString();
	document.cookie = `${name}=${value}; expires=${expires}; path=/`;
}

function getCookie (name) {
	const value = `; ${document.cookie}`;
	const parts = value.split(`; ${name}=`);
	if (parts.length === 2) return parts.pop().split(';').shift();
}