<template>
    <div class="relative" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="fixed inset-0 bg-gray-500/75 transition-opacity" aria-hidden="true"></div>
        <div class="fixed inset-0 w-screen overflow-y-auto">
            <button @click="onClickArea" class="fixed w-screen h-full"></button>
            <div class="flex min-h-full items-center justify-center p-4 text-center sm:items-center sm:p-0">
                <div class="bg-white rounded px-4 pt-5 pb-4 sm:p-4 flex">
                    <div class="mx-auto flex size-16 shrink-0 items-center justify-center 
                            rounded-full sm:mx-0" :class="{
                                'bg-green-200': success,
                                'bg-red-200': !success
                            }">
                        <svg v-show="success" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <g id="SVGRepo_bgCarrier" stroke-width="0"></g>
                            <g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g>
                            <g id="SVGRepo_iconCarrier">
                                <path d="M4 12.6111L8.92308 17.5L20 6.5" stroke="#000000" stroke-opacity="0.75"
                                    stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"></path>
                            </g>
                        </svg>
                        <svg v-show="!success" viewBox="0 -0.5 25 25" fill="none" xmlns="http://www.w3.org/2000/svg"
                            stroke="#000000" stroke-width="0.00025">
                            <g id="SVGRepo_bgCarrier" stroke-width="0"></g>
                            <g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g>
                            <g id="SVGRepo_iconCarrier">
                                <path
                                    d="M6.96967 16.4697C6.67678 16.7626 6.67678 17.2374 6.96967 17.5303C7.26256 17.8232 7.73744 17.8232 8.03033 17.5303L6.96967 16.4697ZM13.0303 12.5303C13.3232 12.2374 13.3232 11.7626 13.0303 11.4697C12.7374 11.1768 12.2626 11.1768 11.9697 11.4697L13.0303 12.5303ZM11.9697 11.4697C11.6768 11.7626 11.6768 12.2374 11.9697 12.5303C12.2626 12.8232 12.7374 12.8232 13.0303 12.5303L11.9697 11.4697ZM18.0303 7.53033C18.3232 7.23744 18.3232 6.76256 18.0303 6.46967C17.7374 6.17678 17.2626 6.17678 16.9697 6.46967L18.0303 7.53033ZM13.0303 11.4697C12.7374 11.1768 12.2626 11.1768 11.9697 11.4697C11.6768 11.7626 11.6768 12.2374 11.9697 12.5303L13.0303 11.4697ZM16.9697 17.5303C17.2626 17.8232 17.7374 17.8232 18.0303 17.5303C18.3232 17.2374 18.3232 16.7626 18.0303 16.4697L16.9697 17.5303ZM11.9697 12.5303C12.2626 12.8232 12.7374 12.8232 13.0303 12.5303C13.3232 12.2374 13.3232 11.7626 13.0303 11.4697L11.9697 12.5303ZM8.03033 6.46967C7.73744 6.17678 7.26256 6.17678 6.96967 6.46967C6.67678 6.76256 6.67678 7.23744 6.96967 7.53033L8.03033 6.46967ZM8.03033 17.5303L13.0303 12.5303L11.9697 11.4697L6.96967 16.4697L8.03033 17.5303ZM13.0303 12.5303L18.0303 7.53033L16.9697 6.46967L11.9697 11.4697L13.0303 12.5303ZM11.9697 12.5303L16.9697 17.5303L18.0303 16.4697L13.0303 11.4697L11.9697 12.5303ZM13.0303 11.4697L8.03033 6.46967L6.96967 7.53033L11.9697 12.5303L13.0303 11.4697Z"
                                    fill="#000000"></path>
                            </g>
                        </svg>
                    </div>
                    <div v-show="props.feedbackText.length > 0" class="text-center my-auto sm:ml-4 sm:mr-4 sm:text-left">
                        <h3 class="align font-bold text-black" id="modal-title">
                            {{ props.feedbackText }}
                        </h3>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { onMounted } from 'vue';
const props = defineProps({
    feedbackText: {
        type: String,
        default: ""
    },
    success: {
        type: Boolean,
        default: false
    },
    showDuration: {
        type: Number,
        default: 2000
    }
})

const emit = defineEmits(["close"])

let closeDelay = null;

onMounted(() => {
    closeDelay = setTimeout(() => {
        clearInterval(closeDelay)
        emit("close")
    }, props.showDuration)
})

function onClickArea() {
    clearTimeout(closeDelay)
    emit("close")
}
</script>