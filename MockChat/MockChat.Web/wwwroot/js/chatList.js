const connection = new signalR.HubConnectionBuilder()
	.withUrl("/hubs/chats")
	.build();

const baseUrl = "http://localhost:5167";

connection.start().then(() => {
	console.log("Connected to the hub");
	connection.invoke("Sync", getCookie("userId"));
}).catch(err => console.error(err.toString()));

connection.on("onChatCreated", (chat) => {
	console.log("Chat created:", chat);
	displayChat(chat);
});

connection.on("onSessionInvalid", sessionInvalid => {
	if (sessionInvalid)  {
		console.error("user is not authorized, redirecting...");
		window.location.href = baseUrl;
	}
});

connection.on("onSync", (user) => {
	console.log("Synced user:", user);
	setCookie("userId", user.id, 365);
});

connection.on("onInvited", chat => {
	displayChat(chat);
	displayToast(chat);
});

function navigateToChat(chatName) {
	let uid = getCookie("userId");
	window.location.href = `${baseUrl}/${uid}/chats/${chatName}`;
}

function fetchChats(currentUserId) {
	fetch(`http://localhost:5167/${currentUserId}/chats`)
		.then(response => response.json())
		.then(chats => {
			console.log("Chats fetched:", chats);
			chats.forEach(chat => displayChat(chat));
		})
		.catch(err => console.error(err.toString()));
}
function displayChat(chat) {
	const chatsList = document.getElementById("chatsList");
	const chatBubble = document.createElement("button");
	chatBubble.classList.add("chat-bubble");
	chatBubble.innerHTML = `<strong>${chat.name}</strong>`;
	chatBubble.addEventListener("click", () => navigateToChat(`${chat.name}`))
	chatsList.appendChild(chatBubble);
}

function displayToast(chat) {
	const toast = document.getElementById("inviteToast");
	const toastText = document.getElementById("inviteToastText");
	toastText.innerHTML = `You have been invited to chat ${chat.name}`;
	const invokeToast = bootstrap.Toast.getOrCreateInstance(toast);
	invokeToast.show();
}

function setCookie(name, value, days) {
	const expires = new Date(Date.now() + days * 24 * 60 * 60 * 1000).toUTCString();
	document.cookie = `${name}=${value}; expires=${expires}; path=/`;
}

function getCookie(name) {
	const value = `; ${document.cookie}`;
	const parts = value.split(`; ${name}=`);
	if (parts.length === 2) return parts.pop().split(';').shift();
}

function createChat(chatName) {
	connection.invoke("CreateChat", chatName);
}

document.getElementById("createChatButton").addEventListener("click", function () {
	const chatName = prompt("Enter chat name:");
	if (chatName) {
		createChat(chatName);
	}
});