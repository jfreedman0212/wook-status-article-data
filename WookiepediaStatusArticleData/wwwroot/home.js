import "/lib/wc-menu-button.min.js";
import "/lib/side-drawer.min.js";

window.addEventListener("load", () => {
    const menuButton = document.querySelector("wc-menu-button");
    const drawer = document.querySelector("side-drawer");

    menuButton.addEventListener("opened", () => {
        drawer.open = true;
    });

    // when actions occur on the drawer, sync the state with the menu button

    drawer.addEventListener("open", () => {
        menuButton.open = true;
    });

    drawer.addEventListener("close", () => {
        menuButton.open = false;
    });
});