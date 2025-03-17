// ...existing code...

// Add event listener for Enter key
document.getElementById("searchInput").addEventListener("keypress", function(event) {
    if (event.key === "Enter") {
        event.preventDefault();
        searchBooks();
    }
});

// ...existing code...
