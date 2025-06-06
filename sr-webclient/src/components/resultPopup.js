import ResultPopupModal from "./ResultPopupModal.vue"
import { spawnComponent } from "./componentSpawner"

export function spawnResultPopup(props = { feedbackText, success, showDuration: 2000 }, zIndex = "100", attachElement = null) {
    const comp = spawnComponent(ResultPopupModal, props, {zIndex: zIndex}, attachElement)
    return comp
}