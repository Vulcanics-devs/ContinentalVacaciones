
import ContinentalLogo from "@/assets/Logo.webp"
import { NavbarUser } from "@/components/ui/navbar-user";

export const Navbar = ({children}: {children: React.ReactNode}) => {

  return (
    <div className="flex justify-between w-full items-center" >
        <div className="p-4">
          <img src={ContinentalLogo} alt="Continental Logo" className="w-48" />
        </div>
        {children}
        <div className="p-4">
          <NavbarUser />
        </div>

    </div>
  )
}
