function togglePassword() {

    let password =
        document.getElementById("password");

    password.type =
        password.type === "password"
        ? "text"
        : "password";
}

function selectRole(role, element) {

    document
        .getElementById("role")
        .value = role;

    document
        .querySelectorAll(".role-btn")
        .forEach(btn =>
            btn.classList.remove("active-role"));

    element.classList.add("active-role");
}