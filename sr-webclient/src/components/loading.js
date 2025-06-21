import LoadingModal from './LoadingModal.vue'
import { spawnComponent } from './componentSpawner'

export function spawnLoading(props = { loadingText }, zIndex = 50, attachElement = null) {
    return spawnComponent(LoadingModal, props, { zIndex: zIndex }, attachElement)
}