console.log("CarRentalSystem Loaded");

function confirmDelete() {

    return confirm(
        "Are you sure you want to delete this car?");
}
/* IMAGE PREVIEW */

function previewImage(event) {

    const image =
        document.getElementById("previewImg");

    const uploadContent =
        document.getElementById("uploadContent");

    image.src =
        URL.createObjectURL(event.target.files[0]);

    image.style.display = "block";

    uploadContent.style.display = "none";
}
/* EDIT IMAGE PREVIEW */

function previewEditImage(event) {

    const image =
        document.getElementById("editPreview");

    image.src =
        URL.createObjectURL(event.target.files[0]);
}