// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
	const searchToggle = document.querySelector(".search-toggle");
	const searchForm = document.getElementById("searchForm");
	const searchInput = document.getElementById("searchInput");

	if (!searchToggle || !searchForm || !searchInput) {
		return;
	}

	searchToggle.addEventListener("click", function () {
		const isHidden = searchForm.hasAttribute("hidden");
		searchForm.toggleAttribute("hidden");
		searchToggle.setAttribute("aria-expanded", String(isHidden));

		if (isHidden) {
			searchInput.focus();
		}
	});
});
