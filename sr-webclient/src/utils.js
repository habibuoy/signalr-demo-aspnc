export { delay, calculatePercentage }

async function delay(durationInMs) {
    return new Promise(resolve => setTimeout(resolve, durationInMs))
}

function calculatePercentage(count, total, decimalPlace = 2) {
    if (typeof count !== 'number'
        || typeof total !== 'number'
        || total === 0) {
        return 0
    }

    return (count / total * 100).toFixed(decimalPlace)
}