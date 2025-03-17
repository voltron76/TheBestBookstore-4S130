// ...existing code...
<TextField
    id="search-box"
    label="Search Book by Title"
    variant="outlined"
    value={searchQuery}
    onChange={(e) => setSearchQuery(e.target.value)}
    onKeyPress={(e) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    }}
    className={classes.searchBox}
/>
// ...existing code...
