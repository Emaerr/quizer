document.addEventListener('DOMContentLoaded', function () {
    const emojis = document.querySelectorAll('.emoji');
    let selectedEmoji = null;

    emojis.forEach(emoji => {
        emoji.addEventListener('click', function () {
            if (selectedEmoji) {
                selectedEmoji.classList.remove('selected');
            }
            selectedEmoji = this;
            selectedEmoji.classList.add('selected');
        });
    })})