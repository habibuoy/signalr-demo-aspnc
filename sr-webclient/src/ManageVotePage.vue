<template>
    <Navbar></Navbar>
    <div :class="['mv-container', showVotes ? '' : 'centered']">
        <div :class="['mv-panel', showVotes ? 'expanded' : 'collapsed']">
            <h2 class="text-2xl font-bold mb-4 text-center">Manage Votes</h2>
            <div class="flex flex-col gap-2">
                <button @click="onClickCreateVote" type="button"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                    Create Vote
                </button>
                <button @click="toggleVoteList" type="button"
                    class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
                    {{ showVotes ? 'Hide votes' : 'Show all votes' }}
                </button>
            </div>

            <div v-if="showVotes" class="mv-list">
                <div v-if="isLoading" class="flex items-center justify-center py-8">
                    <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mr-3"></div>
                    <span class="text-gray-600">Loading votes...</span>
                </div>
                <div v-else-if="votes.length === 0" class="text-center py-8 text-gray-600">
                    No votes available
                </div>
                <div v-else v-for="vote in votes" :key="vote.id"
                    :class="['mv-item', { selected: selectedVoteId === vote.id }]"
                    @click="selectVote(vote.id)">
                    <div class="flex justify-between items-start">
                        <div>
                            <h3 class="text-lg font-semibold">{{ vote.title }}</h3>
                            <button @click.stop="toggleSubjects(vote.id)" 
                                    class="mv-subject-count">
                                {{ vote.subjects.length }} subjects
                            </button>
                        </div>
                        <div class="text-right">
                            <div class="text-sm text-gray-600">
                                Total votes: {{ vote.totalCount }}
                            </div>
                            <div v-if="vote.maximumCount" class="text-sm text-gray-600">
                                Maximum votes: {{ vote.maximumCount }}
                            </div>
                            <div v-if="vote.expiredTime" class="text-sm text-gray-600">
                                Close at: {{ formatExpiry(vote.expiredTime) }}
                            </div>
                        </div>
                    </div>

                    <div :class="['mv-subjects', { expanded: expandedSubjectsId === vote.id }]">
                        <div v-for="subject in vote.subjects" :key="subject.id"
                             class="mv-subject">
                            <span>{{ subject.name }}</span>
                            <span class="text-gray-600">{{ subject.voteCount }} votes 
                                ({{ calculatePercentage(subject.voteCount, vote.totalCount) }}%)</span>
                        </div>
                    </div>

                    <div :class="['mv-actions', { visible: selectedVoteId === vote.id }]">
                        <div class="flex gap-2">
                            <button @click.stop="editVote(vote)"
                                class="bg-blue-500 text-white px-4 py-1 rounded hover:bg-blue-600">
                                Edit
                            </button>
                            <button @click.stop="deleteVote(vote)"
                                class="bg-red-500 text-white px-4 py-1 rounded hover:bg-red-600">
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import { spawnLoading } from './components/loading'
import Navbar from './components/Navbar.vue'
import { delay, calculatePercentage } from './utils'
import { spawnResultPopup } from './components/resultPopup'
import { spawnComponent } from './components/componentSpawner'
import VoteForm from './components/VoteForm.vue'
import { createNewVote, getVotes } from './vote'

let voteForm = null
const showVotes = ref(false)
const selectedVoteId = ref(null)
const expandedSubjectsId = ref(null)
const votes = ref([])
const isLoading = ref(false)

async function fetchVotes() {
    isLoading.value = true
    try {
        await delay(1000) // Simulating network delay
        votes.value = await getVotes(25)
    } catch (error) {
        console.error('Failed to fetch votes:', error)
        spawnResultPopup({
            feedbackText: 'Failed to load votes',
            success: false
        })
    } finally {
        isLoading.value = false
    }
}

function onClickCreateVote() {
    openCreateVoteDialog()
}

function openCreateVoteDialog() {
    voteForm = spawnComponent(VoteForm, { 
        formTitle: "Create a new Vote", closeOnCreate: false, onCreate: onCreate 
    }, { zIndex: "20" })
    voteForm.onDestroy.subscribe(() => voteForm = null)
}

async function toggleVoteList() {
    showVotes.value = !showVotes.value
    if (showVotes.value) {
        await fetchVotes()
    } else {
        selectedVoteId.value = null
        expandedSubjectsId.value = null
    }
}

function selectVote(id) {
    selectedVoteId.value = selectedVoteId.value === id ? null : id
}

function toggleSubjects(id) {
    expandedSubjectsId.value = expandedSubjectsId.value === id ? null : id
}

function formatExpiry(date) {
    return new Date(date).toLocaleString()
}

function editVote(vote) {
    // TODO: Implement edit functionality
    console.log('Edit vote:', vote)
}

function deleteVote(vote) {
    // TODO: Implement delete functionality
    console.log('Delete vote:', vote)
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

    if (showVotes.value) {
        await fetchVotes() // Refresh the list
    }
}
</script>