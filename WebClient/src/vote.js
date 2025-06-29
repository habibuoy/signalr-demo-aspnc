import { httpFetch, HttpMethod, BaseUrl } from './utils'

export {
    Vote, VoteSubject, VoteInput, getVoteInputs, inputVote,
    getVotes, createNewVote, updateVote, deleteVote,
    getFilterOptions
}

const VotesPath = "/votes"
const InputsPath = "/inputs"
const FilterOptionsPath = "/filter-options"
const VotesUrl = BaseUrl + VotesPath
const VoteInputUrl = VotesUrl + InputsPath
const FilterOptionsUrl = VotesUrl + FilterOptionsPath
const UserVoteInputs = BaseUrl + "/users/vote-inputs"

const VotesQueryParam = {
    PAGE: "page",
    COUNT: "count",
    SORTBY: "sortBy",
    SORTORDER: "sortOrder",
    VOTEID: "voteId",
    SUBJECTID: "subjectId",
    SEARCH: "search"
}

async function getVotes(page = 0, count = 10, sortBy, sortOrder, search ) {
    const url = `${VotesUrl}?` +
        `${VotesQueryParam.PAGE}=${page}&` +
        `${VotesQueryParam.COUNT}=${count}&` +
        `${sortBy !== undefined ? `${VotesQueryParam.SORTBY}=${sortBy}&` : ''}`+
        `${sortOrder !== undefined ? `${VotesQueryParam.SORTORDER}=${sortOrder}&` : ''}`+
        `${search !== undefined ? `${VotesQueryParam.SEARCH}=${search}` : ' '}`

    return httpFetch(url)
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.statusCode
                requestResult.validationErrors = response.validationErrors
            } else {
                requestResult.result = response.result.map(vote => {
                    const v = new Vote(
                        vote.id,
                        vote.title,
                        vote.subjects.map(subject => new VoteSubject(
                            subject.id,
                            subject.name,
                            subject.voteCount
                        )),
                        vote.maximumCount,
                        vote.expiredTime,
                        vote.creatorId
                    )
                    v.totalCount = vote.subjects.reduce((sum, subject) => sum + subject.voteCount, 0)
                    return v
                })
            }

            return requestResult
        })
        .catch(error => {
            console.error("Error happened while fetching votes vote", error)
            return { errorMessage: "There was an error getting vote list" }
        })
}

async function getFilterOptions() {
    return httpFetch(FilterOptionsUrl)
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.statusCode
                requestResult.validationErrors = response.validationErrors
            } else {
                requestResult.result = {
                    sortBy: response.result.sortBy,
                    sortOrder: response.result.sortOrder,
                    search: response.result.search
                }
            }

            return requestResult
        })
        .catch(error => {
            console.error("Error happened while fetching votes vote", error)
            return { errorMessage: "There was an error getting vote list" }
        })
}

async function inputVote(voteId, subjectId) {
    return httpFetch(`${VoteInputUrl}?${VotesQueryParam.VOTEID}=${voteId}&${VotesQueryParam.SUBJECTID}=${subjectId}`, HttpMethod.POST)
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                const inp = response.result
                requestResult.result = new VoteInput(
                    inp.id, inp.voteId, inp.voteTitle, inp.subjectId, inp.subjectName, inp.inputTime
                )
            }

            return requestResult
        })
        .catch(error => {
            console.error(`Error happened while inputting on subject ${subjectId} in vote ${voteId}`, error)
            return { errorMessage: "There was an error while inputting vote" }
        })
}

async function getVoteInputs() {
    return httpFetch(`${UserVoteInputs}`)
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                requestResult.result = response.result.map((inp) => new VoteInput(
                    inp.id, inp.voteId, inp.voteTitle, inp.subjectId, inp.subjectName, inp.inputTime
                ))
            }

            return requestResult
        })
        .catch(error => {
            console.error("Error happened while fetching vote inputs", error)
            return { errorMessage: "There was an error while getting vote input list" }
        })
}

async function createNewVote(title, subjects, duration = 0, maxCount = 0) {
    return httpFetch(VotesUrl, HttpMethod.POST,
        new VoteCreate(title, subjects, duration, maxCount))
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                const vote = response.result
                requestResult.result = new Vote(vote.id, vote.title, vote.subjects, vote.maximumCount, vote.expiredTime)
            }

            return requestResult
        })
        .catch(error => {
            console.error(`Error happened while creating new vote ${title}`, error)
            return { errorMessage: "There was an error while creating new vote" }
        })
}

async function updateVote(id, title, subjects, duration, maxCount) {
    return httpFetch(`${VotesUrl}/${id}`, HttpMethod.PUT,
        new VoteUpdateRequest(title, subjects, duration, maxCount))
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                const vote = response.result
                requestResult.result = new Vote(vote.id, vote.title, vote.subjects, vote.maximumCount, vote.expiredTime)
            }

            return requestResult
        })
        .catch(error => {
            console.error(`Error happened while updating vote (${id})`, error)
            return { errorMessage: "There was an error while updating vote" }
        })
}

async function deleteVote(id = "") {
    return httpFetch(`${VotesUrl}/${id}`, HttpMethod.DELETE)
        .then(response => {
            const requestResult = {}
            if (!response.isSuccess) {
                requestResult.errorMessage = response.message
                requestResult.validationErrors = response.validationErrors
            } else {
                requestResult.result = id
            }

            return requestResult
        })
        .catch(error => {
            console.error(`Error happened while deleting vote ${id}`, error)
            return { errorMessage: "There was an error while deleting vote" }
        })
}

class Vote {
    totalCount = 0;

    constructor(id, title, subjects, maxVotes, endTime, creatorId) {
        this.id = id
        this.title = title
        this.subjects = subjects
        this.maximumCount = maxVotes
        this.expiredTime = endTime,
        this.creatorId = creatorId
    }

    canVote() {
        return (!this.expiredTime
            || Date.parse(this.expiredTime) > Date.now()
        )
            && (!this.maximumCount
                || this.totalCount < this.maximumCount
            )
    }
}

class VoteSubject {
    constructor(id, name, voteCount) {
        this.id = id
        this.name = name
        this.voteCount = voteCount
    }
}

class VoteInput {
    constructor(id, voteId, voteTitle, subjectId, subjectName, inputTime) {
        this.id = id
        this.voteId = voteId
        this.voteTitle = voteTitle
        this.subjectId = subjectId
        this.subjectName = subjectName
        this.inputTime = inputTime
    }
}

class VoteCreate {
    constructor(title, subjects, duration = 0, maxCount = 0) {
        this.title = title,
            this.subjects = subjects,
            this.duration = duration > 0 ? duration : null,
            this.maximumCount = maxCount > 0 ? maxCount : null
    }
}

class VoteUpdateRequest {
    constructor(title, subjects, duration, maxCount) {
        this.title = title,
            this.subjects = subjects,
            this.duration = duration > 0 ? duration : null,
            this.maximumCount = maxCount > 0 ? maxCount : null
    }
}