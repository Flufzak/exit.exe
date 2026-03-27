import type {
    GuessRequest,
    GuessResultDto,
    HintResultDto,
    SessionDto,
    StartSessionRequest,
} from "../types/hangman";
import { API_BASE_URL } from "./config";

async function parseResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const text = await response.text();

        if (response.status === 401) {
            throw new Error("UNAUTHORIZED");
        }

        throw new Error(text || `HTTP ${response.status}`);
    }

    return response.json() as Promise<T>;
}

export async function startHangmanSession(
    payload: StartSessionRequest,
): Promise<SessionDto> {
    const response = await fetch(`${API_BASE_URL}/api/sessions`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(payload),
    });

    return parseResponse<SessionDto>(response);
}

export async function getHangmanSession(
    sessionId: string,
): Promise<SessionDto> {
    const response = await fetch(`${API_BASE_URL}/api/sessions/${sessionId}`, {
        method: "GET",
        credentials: "include",
    });

    return parseResponse<SessionDto>(response);
}

export async function submitHangmanGuess(
    sessionId: string,
    payload: GuessRequest,
): Promise<GuessResultDto> {
    const response = await fetch(`${API_BASE_URL}/api/sessions/${sessionId}/guess`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(payload),
    });

    return parseResponse<GuessResultDto>(response);
}

export async function requestHangmanHint(
    sessionId: string,
): Promise<HintResultDto> {
    const response = await fetch(`${API_BASE_URL}/api/sessions/${sessionId}/hint`, {
        method: "POST",
        credentials: "include",
    });

    return parseResponse<HintResultDto>(response);
}