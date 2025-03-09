let sidebarToggleButton;
let appSidebar;
let imgElement;
let headerElement;
let descriptionElement;
let blurredImgElement;

function GetSoundcloudLikeButton() {
    return document.getElementsByClassName("sc-button-like playbackSoundBadge__like sc-button-secondary sc-mr-1x sc-button sc-button-small sc-button-icon sc-button-responsive")[0];
}

function ToggleSidepanel() {
    if (appSidebar) {
        if (appSidebar.style.display == "none") {
            appSidebar.style.display = "block";
        }
        else {
            appSidebar.style.display = "none";
        }
    }
}

let customButtonInCreation = false;
function CreateCustomButton() {
    if (customButtonInCreation) {
        return;
    }

    customButtonInCreation = true;

    sidebarToggleButton = document.createElement("button");
    sidebarToggleButton.style.width = "20px";
    sidebarToggleButton.style.height = "20px";
    sidebarToggleButton.style.background = "none";
    sidebarToggleButton.style.border = "none";
    sidebarToggleButton.style.backgroundRepeat = "no-repeat";
    sidebarToggleButton.style.backgroundImage = "url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGjSURBVDhPhZO/qhpBFMZ/544YNCRcFkzEbSxFsTmd2Aa0MEVewDaFhW0eIXmEVIIgliGXW1jEbdMtiigprYRoE5IoRO64aWYTdjXxgynOme/PwJkjqgpQAd4CL4DH/B974BPwBvgiqloBPgO3aeYVfAMaNy75FrgDioBcOUXgo9O8E1X96Z5dBL46d1Ov1+Mku1qtsNb+zYZnjrsXVY1cUwCMMXS7XQaDAYVCgdFoxGKxAKBWq9Fut2OTCOAmrhwMYETkwREegF9ANgxDACaTSUKQNgBgs9kAsNvtTKvVys7n8x+z2SzrAhI4M7DWsl6v/9RRFDEcDrPL5fIAHBLkSwb/goiYIAgIgiDRPzOoVCqUy+V0247H46Pv+098309cnBkYYyiVSgB4nmd7vd6x2WzmM5lMPpfLHdP8tIHdbrcWyLixZoBHwBGwnufR7/cTAlHVPZAHngNbwHQ6HQA7nU6pVquEYYiIcDqdYl3BcQ+iqnfAS+AD8BrYiQhRFP+vMxSA98Ar4F5UteqW6WmaeQXf42VaAQ3g/tKcL+DguA1g9RvkvXvll3I4BgAAAABJRU5ErkJggg==\")";
    sidebarToggleButton.style.margin = "4px";
    sidebarToggleButton.style.marginTop = "3px";
    sidebarToggleButton.style.marginRight = "0px";
    sidebarToggleButton.style.transform = "scale(0.8)";
    sidebarToggleButton.className = "sc-rpc-client-btn-SIDEBAR-TOGGLE"
    sidebarToggleButton.addEventListener("click", ToggleSidepanel);
    document.getElementsByClassName("playbackSoundBadge__actions")[0].appendChild(sidebarToggleButton);

    if (document.getElementsByClassName("sc-rpc-client-btn-SIDEBAR-TOGGLE")[1]) {
        document.getElementsByClassName("sc-rpc-client-btn-SIDEBAR-TOGGLE")[0].remove();
    }

    appSidebar = document.createElement("div");
    appSidebar.style.position = "fixed";
    appSidebar.style.backgroundColor = "transparent";
    appSidebar.style.borderLeft = "1px solid rgba(128, 128, 128, 0.33)";
    appSidebar.style.width = "25vw";
    appSidebar.style.height = "calc(-127px + 100vh)";
    appSidebar.style.right = "0";
    appSidebar.style.top = "46px";
    appSidebar.style.zIndex = "1005";
    appSidebar.style.padding = "16px";
    appSidebar.style.display = "flex";
    appSidebar.style.flexDirection = "column";
    appSidebar.style.alignItems = "left";
    appSidebar.style.overflow = "hidden";
    appSidebar.className = "sc-rpc-client-btn-SIDEBAR";
    document.body.appendChild(appSidebar);

    if (document.getElementsByClassName("sc-rpc-client-btn-SIDEBAR")[1]) {
        document.getElementsByClassName("sc-rpc-client-btn-SIDEBAR")[0].remove();
    }

    // Create image element
    blurredImgElement = document.createElement("img");
    blurredImgElement.src = GetAlbumCoverLink();
    blurredImgElement.style.width = "100vw";
    blurredImgElement.style.aspectRatio = "1 / 1";
    blurredImgElement.style.objectFit = "cover";
    blurredImgElement.style.margin = "-16px"
    blurredImgElement.style.zIndex = "-222";
    blurredImgElement.style.position = "absolute";
    blurredImgElement.style.display = "unset";
    blurredImgElement.style.transform = "translate(-16.6vw, -16.6vw)"
    blurredImgElement.style.filter = "blur(2vw) brightness(0.33)"
    appSidebar.appendChild(blurredImgElement);

    // Create image element
    imgElement = document.createElement("img");
    imgElement.src = GetAlbumCoverLink().replace(/50x50/g, "500x500");;
    imgElement.style.width = "100%";
    imgElement.style.aspectRatio = "1 / 1";
    imgElement.style.objectFit = "cover";
    imgElement.style.border = "1px solid rgba(128, 128, 128, 0.25)"; // RAHHHHHHH
    imgElement.style.borderRadius = "8px";
    appSidebar.appendChild(imgElement);

    // Create header element
    headerElement = document.createElement("h1");
    headerElement.innerText = GetSongTitle();
    headerElement.style.color = "rgba(220, 220, 220, 0.9)";
    headerElement.style.fontWeight = "bold";
    headerElement.style.fontSize = "24px";
    headerElement.style.marginTop = "10px";
    headerElement.style.marginRight = "6px"
    headerElement.style.paddingRight = "24px";
    appSidebar.appendChild(headerElement);

    // Create description text
    descriptionElement = document.createElement("p");
    descriptionElement.innerText = GetArtistName();
    descriptionElement.style.color = "rgba(220, 220, 220, 0.5)";
    descriptionElement.style.fontSize = "16px";
    descriptionElement.style.paddingRight = "24px";
    appSidebar.appendChild(descriptionElement);

    // default soundcloud like button
    const likeBtn = GetSoundcloudLikeButton();
    secondaryLikeButton = document.createElement("button");
    secondaryLikeButton.style.height = "20px"
    secondaryLikeButton.style.width = "20px"
    secondaryLikeButton.style.right = "16px"
    secondaryLikeButton.style.top = "calc(34px + 25vw)"
    secondaryLikeButton.style.position = "absolute";
    secondaryLikeButton.style.background = "none";
    secondaryLikeButton.style.border = "none";
    secondaryLikeButton.style.backgroundImage = "url(\"data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+DQo8c3ZnIHdpZHRoPSIxNnB4IiBoZWlnaHQ9IjE2cHgiIHZpZXdCb3g9IjAgMCAxNiAxNiIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB4bWxuczpza2V0Y2g9Imh0dHA6Ly93d3cuYm9oZW1pYW5jb2RpbmcuY29tL3NrZXRjaC9ucyI+DQogICAgPCEtLSBHZW5lcmF0b3I6IFNrZXRjaCAzLjAuMyAoNzg5MSkgLSBodHRwOi8vd3d3LmJvaGVtaWFuY29kaW5nLmNvbS9za2V0Y2ggLS0+DQogICAgPHRpdGxlPnN0YXRzX2xpa2VzX2dyZXk8L3RpdGxlPg0KICAgIDxkZXNjPkNyZWF0ZWQgd2l0aCBTa2V0Y2guPC9kZXNjPg0KICAgIDxkZWZzLz4NCiAgICA8ZyBpZD0iUGFnZS0xIiBzdHJva2U9Im5vbmUiIHN0cm9rZS13aWR0aD0iMSIgZmlsbD0ibm9uZSIgZmlsbC1ydWxlPSJldmVub2RkIiBza2V0Y2g6dHlwZT0iTVNQYWdlIj4NCiAgICAgICAgPHBhdGggZD0iTTEwLjgwNDk4MTgsMyBDOC43ODQ3MTU3OSwzIDguMDAwNjUyODUsNS4zNDQ4NjQ4NiA4LjAwMDY1Mjg1LDUuMzQ0ODY0ODYgQzguMDAwNjUyODUsNS4zNDQ4NjQ4NiA3LjIxMjk2Mzg3LDMgNS4xOTYwNDQ5NCwzIEMzLjQ5NDMxMzE4LDMgMS43NDgzNzQsNC4wOTU5MjY5NCAyLjAzMDA4OTk2LDYuNTE0MzA1MzIgQzIuMzczNzI3NjUsOS40NjY3Mzc3NSA3Ljc1NDkxOTE3LDEyLjk5Mjg3MzggNy45OTMxMDk1OCwxMy4wMDEwNTU3IEM4LjIzMTI5OTk4LDEzLjAwOTIzNzggMTMuNzMwOTgyOCw5LjI3ODUzNzggMTMuOTgxNDU5LDYuNTAxMjQwNSBDMTQuMTg3ODY0Nyw0LjIwMDk3MDIzIDEyLjUwNjcxMzYsMyAxMC44MDQ5ODE4LDMgWiIgaWQ9IkltcG9ydGVkLUxheWVycyIgZmlsbD0icmdiKDI1NSwgODUsIDApIiBza2V0Y2g6dHlwZT0iTVNTaGFwZUdyb3VwIi8+DQogICAgPC9nPg0KPC9zdmc+DQo=\")"
    secondaryLikeButton.style.backgroundRepeat = "no-repeat";
    secondaryLikeButton.style.backgroundPosition = "center";
    secondaryLikeButton.style.transform = "scale(1.7)";

    function updateSecondaryLikeButton()
    {
        if (likeBtn.classList.contains("sc-button-selected")) {
            secondaryLikeButton.style.filter = "saturate(1)";
            secondaryLikeButton.style.opacity = "1";
        }
        else {
            secondaryLikeButton.style.filter = "saturate(0)";
            secondaryLikeButton.style.opacity = "0.5";
        }
    }
    updateSecondaryLikeButton();

    secondaryLikeButton.addEventListener('click', function () {
        likeBtn.click();
        updateSecondaryLikeButton();
    });

    likeBtn.addEventListener('click', function () {
        updateSecondaryLikeButton();
    });

    appSidebar.appendChild(secondaryLikeButton);

    customButtonInCreation = false;
}
