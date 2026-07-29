import { SOUTH_HORN } from "./southHorn";
import { NORTH_HORN } from "./northHorn";

export { SOUTH_HORN, NORTH_HORN };

export const ZONES = {
    [SOUTH_HORN.id]: SOUTH_HORN,
    [NORTH_HORN.id]: NORTH_HORN,
};

/*
 * Raw FFXIV territory id (as uploaded by the plugin, TrackerHandler.Territory) ->
 * zone. This is the authoritative way to resolve which zone a tracker describes:
 * the plugin sets Territory from the player's live TerritoryType, so it is always
 * correct, unlike tracker_type (see below).
 */
export const TERRITORY_ZONES = Object.fromEntries(
    Object.values(ZONES).map((zone) => [zone.territory, zone]),
);

/*
 * Stand-in for a tracker the site does not know about yet, so that an
 * unexpected value degrades to "Unknown" labels rather than a broken page.
 */
const UNKNOWN_ZONE = {
    id: "unknown",
    label: "Unknown",
    fates: {},
    encounters: {},
    fateIds: [],
    potFateIds: [],
    encounterIds: [],
    towerId: null,
    towerIcon: null,
    towerSpawnTimer: 3600,
    potRespawn: 1800,
};

/*
 * tracker_type only tells us whether a tracker is fed by the plugin (read-only
 * here) or maintained by hand on the site (password-gated POP/KILL buttons). It
 * never encodes the zone - that comes exclusively from the `territory` field
 * (see TERRITORY_ZONES/getZone below), regardless of which zone the tracker is
 * for or how it was created.
 */
export const TRACKER_TYPES = {
    1: { editable: false },
    2: { editable: true },
};

export const DEFAULT_TRACKER_TYPE = 1;

export function getTrackerType(trackerType) {
    return TRACKER_TYPES[trackerType] ?? null;
}

/*
 * Resolves a tracker's zone from `territory` alone - the raw FFXIV territory id
 * the plugin uploads. `territory` being 0 (the column's default) or unset means
 * the tracker predates this field: every tracker was South Horn back then, so it
 * is treated as South Horn rather than Unknown. Any other unrecognized value
 * still resolves to UNKNOWN_ZONE rather than guessing from tracker_type, which
 * does not reliably encode the zone (see the comment on TRACKER_TYPES).
 */
export function getZone(territory) {
    if (territory === 0 || territory == null) {
        return SOUTH_HORN;
    }
    return TERRITORY_ZONES[territory] ?? UNKNOWN_ZONE;
}

export function isKnownZone(territory) {
    return getZone(territory).id !== UNKNOWN_ZONE.id;
}

export function isEditable(trackerType) {
    return getTrackerType(trackerType)?.editable ?? false;
}

export function isKnownTrackerType(trackerType) {
    return getTrackerType(trackerType) !== null;
}

/*
 * The zones a user can pick when creating a manual tracker, paired with the
 * (single) editable tracker_type. Each option is identified by `territory`,
 * the zone's in-game FFXIV territory id, since every zone shares the same
 * (single) editable tracker_type and so `type` alone cannot distinguish them.
 * A zone whose data file is not filled in yet is still offered: it creates a
 * tracker with empty histories that starts working once the data lands.
 */
export function selectableTrackerTypes() {
    const editableType = Number(
        Object.entries(TRACKER_TYPES).find(([, { editable }]) => editable)[0],
    );
    return Object.values(ZONES).map((zone) => ({
        type: editableType,
        territory: zone.territory,
        zone,
    }));
}
