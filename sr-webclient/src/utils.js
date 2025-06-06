export { delay }

async function delay(durationInMs) {
    return new Promise(resolve => setTimeout(resolve, durationInMs))
}