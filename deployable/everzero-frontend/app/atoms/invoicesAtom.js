import { atom } from "jotai";

export const invoicesAtom = atom({
    invoices: [],
    loading: false,
    loaded: false,
    error: null,
});