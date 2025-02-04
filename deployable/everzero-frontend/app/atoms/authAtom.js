import { atom } from "jotai";

export const authAtom = atom({
    isAuthenticated: false,
    user: null,
    token: null,
});
