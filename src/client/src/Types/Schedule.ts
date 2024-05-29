export type WeekTemplate = {
    id: string,
    name: string,
    site: string,
    items: ScheduleTemplate[]
}

export type ScheduleTemplate = {
    days: DayOfWeek[],
    scheduleBlocks: Session[]
}

export type Session = {
    from: string,
    until: string,
    services: string[]
}

export type DayOfWeek = "Monday" | "Tuesday" | "Wednesday" | "Thursday" | "Friday" | "Saturday" | "Sunday"

export type WeekDaySessionMap = {
    [key in DayOfWeek]: Session[];
}

export type ExplodedWeekTemplate = {
    id: string,
    name: string,
    site: string,
    days: WeekDaySessionMap
}

export type TemplateAssignment = {
    from: string,
    until: string
    templateId: string
}