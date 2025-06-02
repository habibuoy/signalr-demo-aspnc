export { Vote, VoteSubject, VoteInput}

class Vote {
  totalCount = 0;

  constructor(id, title, subjects, maxVotes, endTime) {
    this.id = id
    this.title = title
    this.subjects = subjects
    this.maximumCount = maxVotes
    this.expiredTime = endTime
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