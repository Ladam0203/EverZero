import { atom } from "jotai";

export const reportsAtom = atom({
    reports: [],
    loading: false,
    loaded: false,
    error: null,
});