export type ValidationError = {
    index: number,
    message: string,
    field?: "start" | "end" | "start&end" | "services"
}