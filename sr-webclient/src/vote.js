export { Vote, VoteSubject}

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
      || this.maximumCount < this.subjects.reduce((acc, e) => acc + e, 0)
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
