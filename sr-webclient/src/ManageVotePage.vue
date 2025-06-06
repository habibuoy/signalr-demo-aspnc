<template>
    <Navbar></Navbar>
    <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <div class="bg-white p-8 rounded shadow-md w-full max-w-sm flex flex-col gap-2">
            <h2 class="text-2xl font-bold mb-4 text-center">Manage Votes</h2>
            <button @click="onClickCreateVote" type="button"
                class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                Create Vote
            </button>
            <button type="button" class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                Edit Votes
            </button>
            <button type="button" class="w-full bg-red-500 text-white py-2 rounded hover:bg-red-600">
                Delete Votes
            </button>
        </div>
    </div>
</template>

<script setup>
import { spawnLoading } from './components/loading'
import Navbar from './components/Navbar.vue'
import { delay } from './utils'
import { spawnResultPopup } from './components/resultPopup'
import { spawnComponent } from './components/componentSpawner'
import VoteForm from './components/VoteForm.vue'
import { createNewVote } from './vote'

async function onCreateVote() {
    const loading = spawnLoading({loadingText: "Creating vote..." })

    await delay(2000)
    
    loading.destroy()

    closeCreateVoteDialog()
    const feedback = spawnResultPopup({
        success: true, 
        feedbackText: "Successfully created new vote!",
        showDuration: 2000
    })
}

let voteForm = null

function onClickCreateVote() {
    openCreateVoteDialog()
}

function openCreateVoteDialog() {
    voteForm = spawnComponent(VoteForm, { 
        formTitle: "Create a new Vote", closeOnCreate: false, onCreate: onCreate 
    }, { zIndex: "20" })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

function closeCreateVoteDialog() {
    if (voteForm && voteForm.destroy) {
        voteForm.destroy()
    }
}

async function onCreate(d) {
    if (!d) {
        console.log("no data return from vote create form")
        return
    }

    const loading = spawnLoading({loadingText: "Creating new vote..." }, "20")
    const vote = await createNewVote(d.voteTitle, d.voteSubjects, d.voteDuration, d.voteMaxCount)

    loading.destroy()
    if (voteForm && voteForm.destroy) {
        voteForm.destroy()
    }

    if (!vote) {
        spawnResultPopup({
            feedbackText: `Failed when creating new vote ${d.voteTitle}`,
            success: false
        })
        return
    }

    spawnResultPopup({
        feedbackText: `Succeeded when creating new vote ${d.voteTitle}`,
        success: true
    })
}
</script>