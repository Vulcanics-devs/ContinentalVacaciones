import { useEffect, useMemo, useState } from "react";
import { NavbarUser } from "../ui/navbar-user";
import { useNavigate } from "react-router-dom";
import { ChevronLeft } from "lucide-react";
import CalendarWidget from "../Dashboard-Area/CalendarWidget";
import { areasService } from "@/services/areasService";
import type { Area, Grupo } from "@/interfaces/Areas.interface";
import { toast } from "sonner";

export const Plantilla = () => {
    const navigate = useNavigate();
    const [areas, setAreas] = useState<Area[]>([]);
    const [selectedArea, setSelectedArea] = useState<string>("");
    const [currentGroups, setCurrentGroups] = useState<Grupo[]>([]);
    const [loading, setLoading] = useState<boolean>(false);

    useEffect(() => {
        const loadAreas = async () => {
            try {
                setLoading(true);
                const response = await areasService.getAreas();
                if (Array.isArray(response) && response.length > 0) {
                    setAreas(response);
                    // Iniciar con "all" para mostrar todas las áreas
                    setSelectedArea("all");
                    // Cargar todos los grupos de todas las áreas
                    const allGroups = response.flatMap(area => area.grupos || []);
                    setCurrentGroups(allGroups);
                } else {
                    toast.warning("No hay áreas configuradas para mostrar.");
                }
            } catch (error) {
                console.error("Error cargando áreas:", error);
                toast.error("No se pudieron cargar las áreas");
            } finally {
                setLoading(false);
            }
        };

        loadAreas();
    }, []);

    const handleAreaChange = async (areaId: string) => {
        try {
            setSelectedArea(areaId);

            // Si selecciona "all", mostrar grupos de todas las áreas
            if (areaId === "all") {
                const allGroups = areas.flatMap(area => area.grupos || []);
                setCurrentGroups(allGroups);
                return;
            }

            // Si selecciona un área específica
            const areaInfo = areas.find((a) => a.areaId.toString() === areaId);
            if (areaInfo?.grupos?.length) {
                setCurrentGroups(areaInfo.grupos);
            } else if (areaId) {
                const grupos = await areasService.getGroupsByAreaId(parseInt(areaId));
                setCurrentGroups(grupos);
            } else {
                setCurrentGroups([]);
            }
        } catch (error) {
            console.error("Error cambiando de área:", error);
            toast.error("No se pudieron cargar los grupos del área seleccionada");
            setCurrentGroups([]);
        }
    };

    const selectableAreas = useMemo(
        () => [
            { id: "all", name: "Todas las áreas", manning: 0 },
            ...areas.map((a) => ({
                id: a.areaId.toString(),
                name: a.nombreGeneral,
                manning: a.manning,
            }))
        ],
        [areas]
    );

    const bossName = useMemo(() => {
        if (selectedArea === "all") return "";
        const area = areas.find((a) => a.areaId.toString() === selectedArea);
        return area?.jefe?.fullName || "";
    }, [areas, selectedArea]);

    const filteredGroups = useMemo(() => {
        if (selectedArea === "all") {
            return areas.flatMap(area => area.grupos || []);
        }
        return currentGroups;
    }, [selectedArea, currentGroups, areas]);

    return (
        <div className="flex flex-col min-h-screen w-full bg-white p-12">
            <header className="flex justify-between items-center pb-4">
                <div className="flex flex-col gap-1">
                    <div
                        className="flex items-center gap-2 cursor-pointer"
                        onClick={() => navigate(-1)}
                    >
                        <ChevronLeft /> Regresar
                    </div>
                    <h1 className="text-2xl font-bold  text-slate-800">Roles y plantilla</h1>
                    <p className="text-slate-600">
                        Consulta el calendario de la plantilla por área y grupos.
                    </p>
                </div>
                <NavbarUser />
            </header>
            <CalendarWidget
                showTabs={true}
                defaultView="calendar"
                showHeader={true}
                showSidebar={true}
                showManning={false}
                areas={selectableAreas}
                selectedArea={selectedArea}
                onAreaChange={handleAreaChange}
                currentAreaGroups={currentGroups}
                bossName={bossName}
                className={loading ? "opacity-60 pointer-events-none" : ""}
            />
        </div>
    );
};
