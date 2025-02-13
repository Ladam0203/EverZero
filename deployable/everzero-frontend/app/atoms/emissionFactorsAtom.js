import { atom } from "jotai";

export const emissionFactorsAtom = atom({
    emissionFactors: [],
    loading: false,
    loaded: false,
    error: null,
});