<template>
    <Navbar :onBeforeLogout="onBeforeLogout" />
    <div v-if="isLoading" class="flex items-center justify-center min-h-screen">
        <div class="text-gray-600">Loading...</div>
    </div>
    <div v-else class="min-h-screen py-8">
        <div class="mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8 pt-20">
            <!-- Horizontal Vote List -->
            <div class="mb-8 lg:h-[33vh] sm:h-[40vh]">
                <div class="vote-list" ref="voteList">
                    <div v-for="(vote, index) in votes" :key="vote.id" class="vote-card animate-vertical-slide-in"
                        :id="index" :class="{
                            'ring-2 ring-blue-500': selectedVote?.id === vote.id,
                            'interactable': isListInteractable
                        }" @click="selectVote(vote)">
                        <h3 class="text-lg font-semibold mb-2">{{ vote.title }}</h3>
                        <div class="text-sm text-gray-500">
                            {{ vote.subjects.length }} subjects
                        </div>
                        <div class="text-sm text-black-500">
                            {{ vote.totalCount }} 
                            <span v-if="vote.maximumCount !== null">
                                <strong>/ {{ vote.maximumCount }} </strong>
                            </span> Total Votes
                        </div>
                        <div class="text-sm text-black-500">
                            <span v-if="!vote.expiredTime">No end time</span>
                            <span v-else-if="Date.now() < Date.parse(vote.expiredTime)">Vote ends at: {{ (new
                                Date(vote.expiredTime).toLocaleString()) }}</span>
                            <span v-else>Vote ended</span>
                        </div>
                    </div>
                </div>
            </div>
            <!-- Detailed Vote View -->
            <div v-if="selectedVote" class="animate-slide-in bg-white rounded-lg shadow-sm p-6 mt-4">
                <h2 class="text-2xl font-bold mb-6">{{ selectedVote.title }}</h2>

                <div class="space-y-4 mb-6">
                    <div v-if="selectedVote.maximumCount" class="text-sm text-gray-600">
                        Maximum votes allowed: {{ selectedVote.maximumCount }}
                    </div>
                    <div class="text-sm text-gray-600">
                        Current vote count: {{ selectedVote.totalCount }}
                    </div>
                    <div v-if="selectedVote.expiredTime" class="text-sm text-gray-600">
                        Time remaining: {{ selectedVoteRemainingTime }}
                    </div>
                    <div v-else class="text-sm text-gray-600">
                        No end time
                    </div>
                </div>
                <div class="space-y-4">
                    <div v-for="subject in selectedVote.subjects" :key="subject.id" class="vote-subject">
                        <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-4">
                            <div class="flex-1 min-w-0">
                                <h4 class="text-lg font-medium truncate">{{ subject.name }}</h4>
                                <div class="mt-2">
                                    <div class="w-full bg-gray-200 rounded-full h-2">
                                        <div class="bg-blue-500 h-2 rounded-full transition-all duration-500"
                                            :style="{ width: `${getCurrentVotePercentage(subject.voteCount)}%` }"></div>
                                    </div>
                                    <div class="text-sm text-gray-600 mt-1">
                                        {{ subject.voteCount }} votes ({{ getCurrentVotePercentage(subject.voteCount) }}%)
                                    </div>
                                </div>
                            </div>
                            <button v-if="selectedVote.canVote() && !getVoteInput(selectedVote.id)" 
                                @click="inputVote(selectedVote.id, subject.id)"
                                class="vote-button"
                                :class="{'opacity-50 cursor-not-allowed': inputtingVote}">
                                {{ inputtingVote ? "Voting..." : "Vote" }}
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <div v-else class="text-center text-gray-500">
                Select a vote to view details
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'
import Navbar from './components/Navbar.vue'
import * as signalR from '@microsoft/signalr'
import { Vote, VoteSubject, getVoteInputs, tryInputVote, getVotes } from './vote'
import { delay, calculatePercentage } from './utils'

const votes = ref([])
const selectedVote = ref(null)
const voteList = ref(null)
const isLoading = ref(true)
const isListInteractable = ref(false);
const selectedVoteRemainingTime = ref("");
const currentRemainingTimeIntervalId = ref(null)
const inputtingVote = ref(null)
const isLoggingOut = ref(false)
const voteInputs = ref(null)

// Track user interaction with the vote list
const hasRecentInteraction = ref(false)
const interactionTimeout = 2500 // in milliseconds

// Setup SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7000/watchvote')
    .build()

// Handle SignalR messages
connection.on('NotifyVoteCreated', onVoteCreated)

connection.on('NotifyVoteUpdated', onVoteUpdated)

connection.onreconnecting(async (err) => {
    console.error("Reconnecting", err)
    await login()
})

connection.onreconnected((err) => {
    console.log("Reconnected", err)
})

connection.onclose(async (err) => {
    console.log("Connection closed", err)
    await delay(1000)
    startSignalRConnection()
})

async function startSignalRConnection() {
    try {
        await connection.start()
        console.log("SignalR connected")
    } catch (err) {
        console.error("Error while starting SignalR connection:", err)
    }
}

// Initialize data
onMounted(async () => {
    try {
        await startSignalRConnection()
        await updateVoteInputs()
        loadVotes()
    } catch (error) {
        console.error("Failed to initialize:", error)
    } finally {
        isLoading.value = false
    }
})

async function loadVotes() {
    const votesResult = await getVotes()

    if (!votesResult) {
        console.log("Failed loading votes")
        return
    }

    voteList.value.addEventListener("animationend", onVoteListAnimationEnded)

    for (let i = 0; i < votesResult.length; i++) {
        votes.value.push(votesResult[i])
        await nextTick()
        scrollToEnd()
        await delay(150)
    }
}

async function onVoteListAnimationEnded(anim) {
    if (anim.srcElement.id == (votes.value.length - 1)) {
        await delay(250)
        if (votes.value.length > 0) {
            selectVote(votes.value[0])
            scrollToStart()
        }
        isListInteractable.value = true
        voteList.value.removeEventListener("animationend", onVoteListAnimationEnded)
    }
}

async function inputVote(voteId, subjectId) {
    inputtingVote.value = { voteId: voteId, subjectId: subjectId }

    await tryInputVote(voteId, subjectId)

    await updateVoteInputs()
    inputtingVote.value = null
}

async function updateVoteInputs() {
    voteInputs.value = await getVoteInputs()
}

// Watch voteList ref and setup event listeners
watch(voteList, (newVoteList) => {
    if (newVoteList) {
        newVoteList.addEventListener('mousedown', handleUserInteraction)
        newVoteList.addEventListener('touchstart', handleUserInteraction)
    }
})

// Event listeners cleanup
onUnmounted(() => {
    if (voteList.value) {
        voteList.value.removeEventListener('mousedown', handleUserInteraction)
        voteList.value.removeEventListener('touchstart', handleUserInteraction)
    }
    connection.stop()
})

function onVoteCreated(v) {
    const createdVote = new Vote(
        v.id,
        v.title,
        v.subjects.map(subject => new VoteSubject(
            subject.id,
            subject.name,
            subject.voteCount
        )),
        v.maximumCount,
        v.expiredTime,
    )

    createdVote.totalCount = v.subjects.reduce((sum, subject) => sum + subject.voteCount, 0)
    // console.log(`vote created: ${createdVote.title}`)
    votes.value.unshift(createdVote)
    // Auto-scroll if no recent user interaction
    if (!hasRecentInteraction.value) {
        scrollToStart()
    }
}

function onVoteUpdated(vote) {
    const existingVote = votes.value.find(v => v.id === vote.id)
    // console.log(`vote updated: ${vote.title}`)
    if (existingVote) {
        existingVote.subjects = vote.subjects.map(subject => new VoteSubject(
            subject.id,
            subject.name,
            subject.voteCount
        ));
        existingVote.totalCount = vote.subjects.reduce((sum, subject) => sum + subject.voteCount, 0)
    }
}

function handleUserInteraction() {
    hasRecentInteraction.value = true
    setTimeout(() => {
        hasRecentInteraction.value = false
    }, interactionTimeout)
}

function scrollToStart() {
    if (voteList.value) {
        voteList.value.scrollTo({
            left: 0,
            behavior: 'smooth'
        })
    }
}

function scrollToEnd() {
    if (voteList.value) {
        voteList.value.scrollTo({
            left: voteList.value.scrollWidth - voteList.value.clientWidth,
            behavior: 'smooth'
        })
    }
}

async function selectVote(vote) {
    isListInteractable.value = false
    if (currentRemainingTimeIntervalId.value) {
        clearInterval(currentRemainingTimeIntervalId.value)
        currentRemainingTimeIntervalId.value = null
    }

    if (selectedVote.value) {
        await unsubscribeVote(selectedVote.value)
    }

    if (!vote) return;

    selectedVote.value = vote
    updateSelectedVoteRemainingTime()

    await subscribeVote(vote)

    if (selectedVote.value.expiredTime
        && getRemainingTime(selectedVote.value.expiredTime) > 0) {
        currentRemainingTimeIntervalId.value ??= setInterval(updateSelectedVoteRemainingTime, 1000)
    }
    isListInteractable.value = true
}

async function subscribeVote(vote) {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        return false
    }

    try {
        var result = await connection.invoke("SubscribeVote", vote.id)
        // if (result) {
        //   console.log(`Subscribed to vote ${vote.title} (id: ${vote.id})`)
        // }

        return result
    }
    catch (err) {
        console.error(`Error happened while trying to subscribe to vote ${vote.title}: ${err}`)
    }
}

async function unsubscribeVote(vote) {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        return false
    }

    try {
        var result = await connection.invoke("UnsubscribeVote", vote.id)
        // if (result) {
        //     console.log(`Unsubscribed from vote ${vote.title} (id: ${vote.id})`)
        // }

        return result
    }
    catch (err) {
        console.error(`Error happened while trying to unsubscribe to vote ${vote.title}: ${err}`)
    }
}

function getVoteInput(voteId) {
    if (!voteInputs.value) return null
    return voteInputs.value.find((v) => v.voteId === voteId)
}

function getCurrentVotePercentage(voteCount) {
    if (!selectedVote.value) return 0

    const totalVotes = selectedVote.value.subjects.reduce((sum, subject) => sum + subject.voteCount, 0)
    return calculatePercentage(voteCount, totalVotes)
}

function formatTimeRemaining(expiredTime) {
    if (!expiredTime) return 'No end time'
    const remaining = getRemainingTime(expiredTime)
    if (remaining <= 0) return 'Voting ended'

    const days = Math.floor(remaining / (1000 * 60 * 60 * 24))
    const hours = Math.floor(remaining / (1000 * 60 * 60)) % 24
    const minutes = Math.floor(remaining / (1000 * 60)) % 60
    const seconds = Math.floor(remaining / 1000) % 60

    return `${days > 0 ? days + "d " : ""}${hours}h ${minutes}m ${seconds}s remaining`
}

function getRemainingTime(expiredTime) {
    return new Date(expiredTime) - new Date()
}

function updateSelectedVoteRemainingTime() {
    if (!selectedVote.value) return;
    selectedVoteRemainingTime.value = formatTimeRemaining(selectedVote.value.expiredTime);
    // console.log(`Selected vote ${selectedVote.value.title} remaining time: ${selectedVoteRemainingTime.value}`)
}

async function onBeforeLogout() {
  if (isLoggingOut.value) return
  isLoggingOut.value = true
  // Clean up: unsubscribe, clear interval, disconnect SignalR
  console.log("Cleaning votes")
  if (selectedVote.value) {
    await unsubscribeVote(selectedVote.value)
  }
  if (currentRemainingTimeIntervalId.value) {
    clearInterval(currentRemainingTimeIntervalId.value)
    currentRemainingTimeIntervalId.value = null
  }
  if (connection) {
    console.log("Ending Signal R")
    connection.off("NotifyVoteCreated")
    connection.off("NotifyVoteUpdated")
    connection._closedCallbacks = []
    if (connection.state === signalR.HubConnectionState.Connected) {
      await connection.stop()
    }
    console.log("Signal R ended")
  }
}

onUnmounted(async () => {
    if (voteList.value) {
        voteList.value.removeEventListener('mousedown', handleUserInteraction)
        voteList.value.removeEventListener('touchstart', handleUserInteraction)
    }
})
</script>
