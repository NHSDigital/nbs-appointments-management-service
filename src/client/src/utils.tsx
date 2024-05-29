import React from "react";
import { render } from "@testing-library/react";
import { ISiteContext, SiteContext } from "./ContextProviders/SiteContextProvider";
import { MemoryRouter as Router} from 'react-router-dom';
import { DayOfWeek, ExplodedWeekTemplate, ScheduleTemplate, Session, WeekDaySessionMap, WeekTemplate } from "./Types/Schedule";
import { AuthContext } from "./ContextProviders/AuthContextProvider";

export const wrappedRender = (child: React.ReactNode, siteContext: ISiteContext = {
    site: null,
    siteConfig: null,
    saveSiteConfiguration: jest.fn()
}, signOut: () => void = () => {}) => {

    const getUserEmail = () => "";

    return render(
        <AuthContext.Provider value={{idToken: "123", signOut, getUserEmail}}>
            <SiteContext.Provider value={siteContext}>
                <Router>
                        {child}
                </Router>
            </SiteContext.Provider>
        </AuthContext.Provider>
    )
}

export const parseTime = ( timeString: string, isUntil:boolean = false): null | Date => {
    if (timeString === "") return null;
    var d = new Date();
    var time = timeString.trim().match(/(\d+):(\d\d)/);
    if (time) {
        if(isUntil && time[1].startsWith("00")){
            d.setDate(d.getDate() + 1);
        }
        d.setHours(parseInt(time[1]), parseInt(time[2]));
    } else {
        return null;
    }
    return d;
}

export const blocksEqual = (s1: Session[], s2: Session[]) => {
    return s1.length === s2.length &&
       s1.every((s, i) => s.from === s2[i].from && s.until === s2[i].until && s.services.every((s,j)=> s === s2[i].services[j]));
 }

export const calculateNewSessionTime = (previousUntil: string, defaultTimes: { start: string, end: string }) => {
    //add one hour session
    const returnObject = { ...defaultTimes }
    if (previousUntil) {
        const newStart = parseTime(previousUntil);
        newStart?.setTime(newStart.getTime());
        const newEnd = new Date(newStart!.getTime() + 3600000);
        returnObject.start = newStart!.toTimeString().slice(0, 5);
        returnObject.end = newEnd.toTimeString().slice(0, 5);
    }
    return returnObject;
}

export const templateToViewModel = (weekTemplate: WeekTemplate) : ExplodedWeekTemplate => {
    const days: WeekDaySessionMap = { "Monday": [], "Tuesday": [], "Wednesday": [], "Thursday": [], "Friday": [], "Saturday": [], "Sunday": [] };
    weekTemplate.items.forEach((sT) => {
        sT.days.forEach(day => {
            // map to break reference to original array values
            days[day] = sT.scheduleBlocks.map(x => ({ ...x }));
        });
    });
    return {
        id: weekTemplate.id,
        name: weekTemplate.name,
        site: weekTemplate.site,
        days
    }
}

export const cloneTemplate = (original: ExplodedWeekTemplate) : ExplodedWeekTemplate => {
    const clone = {
        id: original.id,
        name: original.name,
        site: original.site,
        days: { Monday: [], Tuesday: [], Wednesday: [], Thursday: [], Friday: [], Saturday: [], Sunday: [] } as WeekDaySessionMap
    }

    Object.keys(clone.days).forEach(key => {
        clone.days[key as DayOfWeek] = original.days[key as DayOfWeek].map(x => ({ ...x }));
    });

    return clone;
}

export const viewModelToTemplate = (state: ExplodedWeekTemplate) : WeekTemplate => {
    const templates: ScheduleTemplate[] = [];
    Object.entries(state.days).forEach(([day, blocks]) => {
        const match = templates.find(t => blocksEqual(t.scheduleBlocks, blocks));
        if (match) {
            match.days.push(day as DayOfWeek);
        } else {
            if (blocks.length) {
                templates.push({ days: [day as DayOfWeek], scheduleBlocks: blocks });
            }
        }
    });
    return {
        id: state.id,
        site: state.site,
        name: state.name,
        items: templates
    }
}