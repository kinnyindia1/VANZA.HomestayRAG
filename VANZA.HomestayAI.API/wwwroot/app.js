
const conversationList =   document.getElementById("conversationList");
const chatMessages =  document.getElementById("chatMessages");
const chatForm =  document.getElementById("chatForm");
const messageInput =  document.getElementById("messageInput");
const sendBtn =  document.getElementById("sendBtn");
const newChatBtn =  document.getElementById("newChatBtn");
let currentChatId = null;

// Load all previous conversations
async function loadConversations() {
    try {
        const response = await fetch("/api/chat/conversations");
        if (!response.ok) {
            throw new Error("Unable to load conversations.");
        }
        const conversations = await response.json();
        conversationList.replaceChildren();
        conversations.forEach(conversation => {
            const button = document.createElement("button");
            button.className = "conversation-item";
            button.textContent = conversation.title || "New Chat";
            button.classList.toggle(
                "active",
                conversation.chatId === currentChatId
            );
            button.addEventListener("click", () => {
                openConversation(conversation.chatId);
            });
            conversationList.appendChild(button);
        });
    }
    catch (error) {
        console.error(error);
    }
}
// Display a message
function addMessage(role, content, generatedSql = null) {
    const message = document.createElement("div");
    message.className = `message ${role}`;
    const label = document.createElement("div");
    label.className = "message-label";
    label.textContent = role === "user" ? "YOU" : "VANZA AI";
    const text = document.createElement("div");
    text.textContent = content;
    message.appendChild(label);
    message.appendChild(text);
    if (generatedSql) {
        const details = document.createElement("details");
        details.className = "sql-details";
        const summary = document.createElement("summary");
        summary.textContent = "View generated SQL";
        const pre = document.createElement("pre");
        pre.textContent = generatedSql;
        details.appendChild(summary);
        details.appendChild(pre);
        message.appendChild(details);
    }
    chatMessages.appendChild(message);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}
// Open a previous conversation
async function openConversation(chatId) {
    try {
        const response = await fetch(
            `/api/chat/conversations/${chatId}`
        );
        if (!response.ok) {
            throw new Error("Unable to open conversation.");
        }
        const conversation = await response.json();
        currentChatId = conversation.chatId;
        chatMessages.replaceChildren();
        conversation.messages.forEach(message => {
            addMessage(
                message.role,
                message.content,
                message.generatedSql
            );
        });
        await loadConversations();
    }
    catch (error) {
        alert(error.message);
    }
}
// Start a new conversation
function newChat() {
    currentChatId = null;
    chatMessages.replaceChildren();
    const welcome = document.createElement("div");
    welcome.className = "welcome";
    const heading = document.createElement("h1");
    heading.textContent = "Welcome to VANZA AI";
    const paragraph = document.createElement("p");
    paragraph.textContent = "Ask questions about your homestay data and documents.";
    welcome.appendChild(heading);
    welcome.appendChild(paragraph);
    chatMessages.appendChild(welcome);
    loadConversations();
}
// Send a question
chatForm.addEventListener("submit", async function (event) {
    event.preventDefault();
    const message = messageInput.value.trim();
    if (!message) return;
    addMessage("user", message);
    messageInput.value = "";
    sendBtn.disabled = true;
    sendBtn.textContent = "Thinking...";
    try {
        const response = await fetch("/api/chat/ask", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                chatId: currentChatId,
                message: message,
                mode: "auto"
            })
        });
        const result = await response.json();
        if (!response.ok) {           
               throw new Error(
                   result.error || result.message || "Unable to process your question."               
            );
        }
        currentChatId = result.chatId;
        addMessage(
            "assistant",
            result.answer,
            result.generatedSql
        );
        await loadConversations();
    }
    catch (error) {
        addMessage("assistant", "Error: " + error.message);
    }
    finally {
        sendBtn.disabled = false;
        sendBtn.textContent = "Send";
        messageInput.focus();
    }
});
// New chat button
newChatBtn.addEventListener("click", newChat);
// Initial load
loadConversations();
