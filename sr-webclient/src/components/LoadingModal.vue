<template>
    <div class="relative" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="fixed inset-0 bg-gray-500/75 transition-opacity" aria-hidden="true"></div>
        <div class="fixed inset-0 w-screen overflow-y-auto">
            <div class="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
                <div
                    class="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg">
                    <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                        <div class="flex">
                            <div
                                class="animate-spin mx-auto flex size-12 shrink-0 items-center justify-center 
                                    rounded-full bg-blue-100 sm:mx-0 sm:size-10">
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200">
                                    <radialGradient id="a9" cx=".66" fx=".66" cy=".3125" fy=".3125" gradientTransform="scale(1.5)">
                                        <stop offset="0" stop-color="#303538"></stop>
                                        <stop offset=".3" stop-color="#303538" stop-opacity=".9"></stop>
                                        <stop offset=".6" stop-color="#303538" stop-opacity=".6"></stop>
                                        <stop offset=".8" stop-color="#303538" stop-opacity=".3"></stop>
                                        <stop offset="1" stop-color="#303538" stop-opacity="0"></stop>
                                    </radialGradient>
                                    <circle transform-origin="center" fill="none" stroke="url(#a9)" stroke-width="15" stroke-linecap="round" stroke-dasharray="200 1000" stroke-dashoffset="0" cx="100" cy="100" r="70">
                                    </circle>
                                    <circle transform-origin="center" fill="none" opacity=".2" stroke="#303538" stroke-width="15" stroke-linecap="round" cx="100" cy="100" r="70"></circle>
                                </svg>
                            </div>
                            <div class="text-center my-auto sm:ml-4 sm:mr-4 sm:text-left">
                                <h3 class="align font-semibold text-gray-900" id="modal-title">
                                    {{ props.loadingText.length == 0 ? "Loading..." : props.loadingText }}
                                </h3>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
const props = defineProps({
    loadingText: {
        type: String,
        default: "Loading..."
    }
})
</script>